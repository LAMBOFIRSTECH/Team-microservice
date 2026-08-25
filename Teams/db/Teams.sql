-- ============================================================================
-- PostgreSQL Schema: Teams Domain-Driven Architecture
-- Fully Aligned with CQRS / Domain Rules & Outbox Pattern
-- ============================================================================

CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE SCHEMA IF NOT EXISTS teams;

/* ============================================================================
   1. TEAMS (Aggregate Root)
   ============================================================================ */

CREATE TABLE teams.teams (
    id                     uuid              NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    name                   varchar(150)      NOT NULL,
    team_manager_id        uuid              NOT NULL,
    state                  smallint          NOT NULL DEFAULT 0,     -- 0=Draft, 1=Active, 2=Archived
    average_productivity   double precision  NOT NULL DEFAULT 0,
    taux_turnover          double precision  NOT NULL DEFAULT 0,
    composition_hash       bytea             NULL,                   -- SHA256 hash of sorted member IDs
    team_creation_date     timestamptz       NOT NULL DEFAULT clock_timestamp(),
    last_activity_date     timestamptz       NOT NULL DEFAULT clock_timestamp(),
    team_expiration_date   timestamptz       NOT NULL,
    extra_days             integer           NOT NULL DEFAULT 0,
    is_deleted             boolean           NOT NULL DEFAULT false,

    CONSTRAINT ck_teams_name_not_empty CHECK (length(btrim(name)) > 0),
    CONSTRAINT ck_teams_state CHECK (state IN (0, 1, 2))
);

-- Active teams must have unique names
CREATE UNIQUE INDEX uq_teams_name_actives ON teams.teams(name) WHERE is_deleted = false;
CREATE INDEX ix_teams_state ON teams.teams(state) WHERE is_deleted = false;

-- Anti-duplicate team composition constraint (Rule H)
CREATE UNIQUE INDEX uq_teams_composition_hash 
    ON teams.teams(composition_hash) 
    WHERE is_deleted = false AND state <> 2 AND composition_hash IS NOT NULL;

/* ============================================================================
   2. TEAM MEMBERS & HISTORY
   ============================================================================ */

CREATE TABLE teams.team_members (
    team_id   uuid NOT NULL,
    member_id uuid NOT NULL,

    CONSTRAINT pk_team_members PRIMARY KEY (team_id, member_id),
    CONSTRAINT fk_team_members_teams FOREIGN KEY (team_id) REFERENCES teams.teams(id) ON DELETE CASCADE
);

CREATE INDEX ix_team_members_member_id ON teams.team_members(member_id);

CREATE TABLE teams.team_members_history (
    id         uuid        NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    team_id    uuid        NOT NULL,
    member_id  uuid        NOT NULL,
    join_date  timestamptz NOT NULL DEFAULT clock_timestamp(),
    leave_date timestamptz NULL,

    CONSTRAINT fk_tmh_teams FOREIGN KEY (team_id) REFERENCES teams.teams(id) ON DELETE CASCADE
);

CREATE INDEX ix_tmh_member_open ON teams.team_members_history(member_id, leave_date);
CREATE INDEX ix_tmh_team ON teams.team_members_history(team_id);

/* ============================================================================
   3. PROJECT ASSOCIATIONS
   ============================================================================ */

CREATE TABLE teams.project_associations (
    team_id          uuid          NOT NULL PRIMARY KEY,
    project_id       uuid          NOT NULL,
    team_manager_id  uuid          NOT NULL,
    team_name        varchar(150)  NOT NULL,
    state            smallint      NOT NULL,   -- 0=Unassigned, 1=Assigned, 2=Suspended, 3=UnderReview, 4=UnassignedAfterReview
    is_under_review  boolean       NOT NULL DEFAULT false,

    CONSTRAINT fk_pa_teams FOREIGN KEY (team_id) REFERENCES teams.teams(id),
    CONSTRAINT ck_pa_state CHECK (state IN (0, 1, 2, 3, 4))
);

CREATE TABLE teams.project_association_details (
    id             integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    team_id        uuid          NOT NULL,
    project_name   varchar(150)  NOT NULL,
    start_date     timestamptz   NOT NULL,
    end_date       timestamptz   NOT NULL,
    state          smallint      NOT NULL,   -- 0=Active, 1=Suspended
    suspended_at   timestamptz   NULL,

    CONSTRAINT fk_pad_pa FOREIGN KEY (team_id) REFERENCES teams.project_associations(team_id) ON DELETE CASCADE,
    CONSTRAINT ck_pad_end_after_start CHECK (end_date > start_date),
    CONSTRAINT ck_pad_state CHECK (state IN (0, 1)),
    CONSTRAINT ck_pad_suspended_at_coherence CHECK (
        (state = 1 AND suspended_at IS NOT NULL) OR
        (state = 0 AND suspended_at IS NULL)
    )
);

CREATE INDEX ix_pad_team ON teams.project_association_details(team_id);

/* ============================================================================
   4. TRANSACTIONAL OUTBOX & EVENT DEAD-LETTER
   ============================================================================ */

CREATE TABLE teams.team_events (
    id             uuid          NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    team_id        uuid          NOT NULL,
    event_type     varchar(60)   NOT NULL,
    payload_json   jsonb         NOT NULL,
    exchange       varchar(100)  NOT NULL DEFAULT 'teams.events',
    routing_key    varchar(200)  NOT NULL DEFAULT '',
    correlation_id uuid          NULL,
    occurred_on    timestamptz   NOT NULL DEFAULT clock_timestamp(),
    processed      boolean       NOT NULL DEFAULT false,
    processed_at   timestamptz   NULL,
    retry_count    integer       NOT NULL DEFAULT 0,
    last_error     text          NULL,
    next_retry_at  timestamptz   NULL,

    CONSTRAINT ck_team_events_type CHECK (event_type IN (
        'TeamCreated', 'MemberAdded', 'MemberRemoved', 'ManagerChanged',
        'ProjectAssigned', 'ProjectSuspendedFromTeam', 'ProjectRemovedFromTeam',
        'TeamArchived', 'TeamDeleted')),
    CONSTRAINT fk_team_events_teams FOREIGN KEY (team_id) REFERENCES teams.teams(id)
);

CREATE INDEX ix_team_events_unprocessed 
    ON teams.team_events(occurred_on) 
    WHERE processed = false;

CREATE TABLE teams.team_events_dead_letter (
    id               uuid         NOT NULL PRIMARY KEY,
    team_id          uuid         NOT NULL,
    event_type       varchar(60)  NOT NULL,
    payload_json     jsonb        NOT NULL,
    exchange         varchar(100) NOT NULL,
    routing_key      varchar(200) NOT NULL,
    correlation_id   uuid         NULL,
    occurred_on      timestamptz  NOT NULL,
    retry_count      integer      NOT NULL,
    last_error       text         NULL,
    dead_lettered_at timestamptz  NOT NULL DEFAULT clock_timestamp()
);

/* ============================================================================
   5. AUDIT & METRICS HISTORY
   ============================================================================ */

CREATE TABLE teams.audit_log (
    id          uuid         NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    team_id     uuid         NOT NULL,
    action      varchar(50)  NOT NULL,
    old_value   jsonb        NULL,
    new_value   jsonb        NULL,
    user_id     uuid         NULL,
    action_date timestamptz  NOT NULL DEFAULT clock_timestamp()
);

CREATE TABLE teams.team_productivity_history (
    id           uuid             NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    team_id      uuid             NOT NULL,
    productivity double precision NOT NULL,
    measured_at  timestamptz      NOT NULL DEFAULT clock_timestamp(),

    CONSTRAINT fk_tph_teams FOREIGN KEY (team_id) REFERENCES teams.teams(id) ON DELETE CASCADE
);

/* ============================================================================
   6. TRIGGERS & BUSINESS INVARIANTS
   ============================================================================ */

-- 6.1 Default Outbox Routing Key
CREATE OR REPLACE FUNCTION teams.fn_team_events_default_routing()
RETURNS trigger AS $$
BEGIN
    IF NEW.routing_key = '' THEN
        NEW.routing_key := NEW.event_type;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_team_events_default_routing
BEFORE INSERT ON teams.team_events
FOR EACH ROW
EXECUTE FUNCTION teams.fn_team_events_default_routing();

-- 6.2 Prevent updates to archived teams
CREATE OR REPLACE FUNCTION teams.fn_teams_block_update_if_archived()
RETURNS trigger AS $$
BEGIN
    IF OLD.state = 2 THEN
        RAISE EXCEPTION 'Cannot update team %: archived teams are read-only.', OLD.id;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_teams_block_update_if_archived
BEFORE UPDATE ON teams.teams
FOR EACH ROW
EXECUTE FUNCTION teams.fn_teams_block_update_if_archived();

-- 6.3 Validate member insertion (Max 10, cooldown, active team)
CREATE OR REPLACE FUNCTION teams.fn_team_members_validate_on_add()
RETURNS trigger AS $$
DECLARE
    v_state smallint;
    v_count integer;
BEGIN
    SELECT state INTO v_state FROM teams.teams WHERE id = NEW.team_id;
    IF v_state = 2 THEN
        RAISE EXCEPTION 'Cannot add member: team % is archived.', NEW.team_id;
    END IF;

    SELECT count(*) INTO v_count FROM teams.team_members WHERE team_id = NEW.team_id;
    IF v_count >= 10 THEN
        RAISE EXCEPTION 'Cannot add member: team % has reached the maximum capacity of 10 members.', NEW.team_id;
    END IF;

    IF EXISTS (
        SELECT 1 FROM teams.team_members_history h
        WHERE h.member_id = NEW.member_id
          AND h.team_id <> NEW.team_id
          AND h.leave_date IS NOT NULL
          AND h.leave_date >= clock_timestamp() - interval '7 days'
    ) THEN
        RAISE EXCEPTION 'Cannot add member %: user is on a 7-day cooldown period after leaving another team.', NEW.member_id;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_team_members_validate_on_add
BEFORE INSERT ON teams.team_members
FOR EACH ROW
EXECUTE FUNCTION teams.fn_team_members_validate_on_add();

-- 6.4 Maintain Membership History
CREATE OR REPLACE FUNCTION teams.fn_team_members_history_on_add()
RETURNS trigger AS $$
BEGIN
    INSERT INTO teams.team_members_history(team_id, member_id, join_date)
    VALUES (NEW.team_id, NEW.member_id, clock_timestamp());
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_team_members_history_on_add
AFTER INSERT ON teams.team_members
FOR EACH ROW
EXECUTE FUNCTION teams.fn_team_members_history_on_add();

CREATE OR REPLACE FUNCTION teams.fn_team_members_history_on_remove()
RETURNS trigger AS $$
BEGIN
    UPDATE teams.team_members_history h
    SET leave_date = clock_timestamp()
    WHERE h.id = (
        SELECT h2.id FROM teams.team_members_history h2
        WHERE h2.team_id = OLD.team_id AND h2.member_id = OLD.member_id AND h2.leave_date IS NULL
        ORDER BY h2.join_date DESC
        LIMIT 1
    );
    RETURN OLD;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_team_members_history_on_remove
AFTER DELETE ON teams.team_members
FOR EACH ROW
EXECUTE FUNCTION teams.fn_team_members_history_on_remove();

-- 6.5 Recalculate State & Composition Hash
CREATE OR REPLACE FUNCTION teams.fn_teams_recalculate_state_and_hash()
RETURNS trigger AS $$
DECLARE
    v_team_id uuid := COALESCE(NEW.team_id, OLD.team_id);
    v_nb_membres integer;
    v_is_manager_member boolean;
    v_hash bytea;
BEGIN
    SELECT 
        count(*),
        digest(string_agg(member_id::text, ',' ORDER BY member_id), 'sha256')
    INTO v_nb_membres, v_hash
    FROM teams.team_members 
    WHERE team_id = v_team_id;

    SELECT EXISTS (
        SELECT 1 FROM teams.team_members m
        JOIN teams.teams t ON t.id = v_team_id
        WHERE m.team_id = v_team_id AND m.member_id = t.team_manager_id
    ) INTO v_is_manager_member;

    UPDATE teams.teams t
    SET state = CASE
                    WHEN t.state = 2 THEN 2
                    WHEN v_nb_membres >= 3 AND v_is_manager_member THEN 1
                    ELSE 0
                END,
        composition_hash = v_hash,
        last_activity_date = clock_timestamp()
    WHERE t.id = v_team_id;

    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_teams_recalculate_state_and_hash
AFTER INSERT OR DELETE ON teams.team_members
FOR EACH ROW
EXECUTE FUNCTION teams.fn_teams_recalculate_state_and_hash();

-- 6.6 Deferred Constraint Trigger: Ensure Manager is a Team Member
CREATE OR REPLACE FUNCTION teams.fn_teams_check_manager_is_member()
RETURNS trigger AS $$
BEGIN
    IF NEW.is_deleted = false AND NEW.state <> 2 THEN
        IF NOT EXISTS (
            SELECT 1 FROM teams.team_members m
            WHERE m.team_id = NEW.id AND m.member_id = NEW.team_manager_id
        ) THEN
            RAISE EXCEPTION 'Team manager (ID: %) must be an active member of team (ID: %).', NEW.team_manager_id, NEW.id;
        END IF;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE CONSTRAINT TRIGGER trg_teams_check_manager_is_member
AFTER INSERT OR UPDATE OF team_manager_id ON teams.teams
DEFERRABLE INITIALLY DEFERRED
FOR EACH ROW
EXECUTE FUNCTION teams.fn_teams_check_manager_is_member();

-- 6.7 Manager Workload Check (Max 3 teams managed concurrently)
CREATE OR REPLACE FUNCTION teams.fn_teams_check_manager_workload()
RETURNS trigger AS $$
DECLARE
    v_count integer;
BEGIN
    IF TG_OP = 'INSERT' OR NEW.team_manager_id IS DISTINCT FROM OLD.team_manager_id THEN
        SELECT count(*) INTO v_count
        FROM teams.teams
        WHERE team_manager_id = NEW.team_manager_id
          AND state <> 2 AND is_deleted = false
          AND id <> NEW.id;

        IF v_count >= 3 THEN
            RAISE EXCEPTION 'Manager (ID: %) cannot manage more than 3 active or draft teams concurrently.', NEW.team_manager_id;
        END IF;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_teams_check_manager_workload
BEFORE INSERT OR UPDATE OF team_manager_id ON teams.teams
FOR EACH ROW
EXECUTE FUNCTION teams.fn_teams_check_manager_workload();

-- 6.8 Limit Max 3 Project Association Details
CREATE OR REPLACE FUNCTION teams.fn_pad_max_three()
RETURNS trigger AS $$
DECLARE
    v_count integer;
BEGIN
    SELECT count(*) INTO v_count FROM teams.project_association_details WHERE team_id = NEW.team_id;
    IF v_count > 3 THEN
        RAISE EXCEPTION 'Team (ID: %) cannot have more than 3 project details associated.', NEW.team_id;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_pad_max_three
AFTER INSERT ON teams.project_association_details
FOR EACH ROW
EXECUTE FUNCTION teams.fn_pad_max_three();

-- 6.9 Audit Log Triggers
CREATE OR REPLACE FUNCTION teams.fn_audit_teams()
RETURNS trigger AS $$
BEGIN
    INSERT INTO teams.audit_log(team_id, action, old_value, new_value)
    VALUES (
        NEW.id, 'UPDATE_TEAM',
        jsonb_build_object('name', OLD.name, 'team_manager_id', OLD.team_manager_id, 'state', OLD.state),
        jsonb_build_object('name', NEW.name, 'team_manager_id', NEW.team_manager_id, 'state', NEW.state)
    );
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_audit_teams
AFTER UPDATE ON teams.teams
FOR EACH ROW
EXECUTE FUNCTION teams.fn_audit_teams();

CREATE OR REPLACE FUNCTION teams.fn_audit_team_members_insert()
RETURNS trigger AS $$
BEGIN
    INSERT INTO teams.audit_log(team_id, action, new_value)
    VALUES (NEW.team_id, 'MEMBER_ADDED', jsonb_build_object('member_id', NEW.member_id));
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_audit_team_members_insert
AFTER INSERT ON teams.team_members
FOR EACH ROW
EXECUTE FUNCTION teams.fn_audit_team_members_insert();

CREATE OR REPLACE FUNCTION teams.fn_audit_team_members_delete()
RETURNS trigger AS $$
BEGIN
    INSERT INTO teams.audit_log(team_id, action, old_value)
    VALUES (OLD.team_id, 'MEMBER_REMOVED', jsonb_build_object('member_id', OLD.member_id));
    RETURN OLD;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_audit_team_members_delete
AFTER DELETE ON teams.team_members
FOR EACH ROW
EXECUTE FUNCTION teams.fn_audit_team_members_delete();

/* ============================================================================
   7. DOMAIN STORED PROCEDURES
   ============================================================================ */

-- 7.1 Create Team
CREATE OR REPLACE FUNCTION teams.sp_team_creer(
    p_name              varchar(150),
    p_team_manager_id   uuid,
    p_validity_days     integer DEFAULT 250
) RETURNS uuid AS $$
DECLARE
    v_team_id uuid;
    v_now     timestamptz := clock_timestamp();
BEGIN
    IF EXISTS (SELECT 1 FROM teams.teams WHERE name = p_name AND is_deleted = false) THEN
        RAISE EXCEPTION 'A team with the name "%" already exists.', p_name;
    END IF;

    v_team_id := gen_random_uuid();

    INSERT INTO teams.teams (
        id, name, team_manager_id, state, team_creation_date, last_activity_date, team_expiration_date
    ) VALUES (
        v_team_id, p_name, p_team_manager_id, 0, v_now, v_now, v_now + make_interval(days => p_validity_days)
    );

    -- Automatically add manager as a member
    INSERT INTO teams.team_members (team_id, member_id) VALUES (v_team_id, p_team_manager_id);

    INSERT INTO teams.team_events (team_id, event_type, payload_json)
    VALUES (v_team_id, 'TeamCreated', jsonb_build_object('team_id', v_team_id, 'team_manager_id', p_team_manager_id));

    RETURN v_team_id;
END;
$$ LANGUAGE plpgsql;

-- 7.2 Add / Remove Member
CREATE OR REPLACE FUNCTION teams.sp_team_ajouter_membre(
    p_team_id               uuid,
    p_member_id             uuid,
    p_requesting_user_id    uuid,
    p_requesting_user_role  varchar(20)
) RETURNS void AS $$
DECLARE
    v_manager_id uuid;
BEGIN
    SELECT team_manager_id INTO v_manager_id FROM teams.teams WHERE id = p_team_id;

    IF p_requesting_user_role <> 'Admin' AND p_requesting_user_id <> v_manager_id THEN
        RAISE EXCEPTION 'Permission denied: only the team manager or an administrator can add members.';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM teams.team_members WHERE team_id = p_team_id AND member_id = p_member_id) THEN
        INSERT INTO teams.team_members (team_id, member_id) VALUES (p_team_id, p_member_id);
    END IF;

    INSERT INTO teams.team_events (team_id, event_type, payload_json)
    VALUES (p_team_id, 'MemberAdded', jsonb_build_object('team_id', p_team_id, 'member_id', p_member_id));
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION teams.sp_team_retirer_membre(
    p_team_id               uuid,
    p_member_id             uuid,
    p_requesting_user_id    uuid,
    p_requesting_user_role  varchar(20)
) RETURNS void AS $$
DECLARE
    v_manager_id uuid;
BEGIN
    SELECT team_manager_id INTO v_manager_id FROM teams.teams WHERE id = p_team_id;

    IF p_requesting_user_role <> 'Admin' AND p_requesting_user_id <> v_manager_id THEN
        RAISE EXCEPTION 'Permission denied: only the team manager or an administrator can remove members.';
    END IF;

    IF v_manager_id = p_member_id THEN
        RAISE EXCEPTION 'Cannot remove member: the active team manager cannot be removed until replaced.';
    END IF;

    DELETE FROM teams.team_members WHERE team_id = p_team_id AND member_id = p_member_id;

    INSERT INTO teams.team_events (team_id, event_type, payload_json)
    VALUES (p_team_id, 'MemberRemoved', jsonb_build_object('team_id', p_team_id, 'member_id', p_member_id));
END;
$$ LANGUAGE plpgsql;

-- 7.3 Change Manager
CREATE OR REPLACE FUNCTION teams.sp_team_changer_manager(
    p_team_id          uuid,
    p_new_manager_id   uuid
) RETURNS void AS $$
DECLARE
    v_old_manager uuid;
BEGIN
    SELECT team_manager_id INTO v_old_manager FROM teams.teams WHERE id = p_team_id;

    -- New manager must be a member
    IF NOT EXISTS (SELECT 1 FROM teams.team_members WHERE team_id = p_team_id AND member_id = p_new_manager_id) THEN
        RAISE EXCEPTION 'Cannot change manager: proposed manager (ID: %) is not a member of team %.', p_new_manager_id, p_team_id;
    END IF;

    UPDATE teams.teams SET team_manager_id = p_new_manager_id WHERE id = p_team_id;

    INSERT INTO teams.team_events (team_id, event_type, payload_json)
    VALUES (p_team_id, 'ManagerChanged',
            jsonb_build_object('team_id', p_team_id, 'old_manager_id', v_old_manager, 'new_manager_id', p_new_manager_id));
END;
$$ LANGUAGE plpgsql;

-- 7.4 Assign Project
CREATE OR REPLACE FUNCTION teams.sp_team_assigner_projet(
    p_team_id       uuid,
    p_project_id    uuid,
    p_project_name  varchar(150),
    p_start_date    timestamptz,
    p_end_date      timestamptz
) RETURNS void AS $$
DECLARE
    v_manager_id   uuid;
    v_name         varchar(150);
    v_team_created timestamptz;
    v_state        smallint;
BEGIN
    SELECT team_manager_id, name, team_creation_date, state
    INTO v_manager_id, v_name, v_team_created, v_state
    FROM teams.teams WHERE id = p_team_id;

    IF v_state <> 1 THEN
        RAISE EXCEPTION 'Cannot assign project: team % must be in Active state (1).', p_team_id;
    END IF;

    IF EXISTS (SELECT 1 FROM teams.project_associations WHERE team_id = p_team_id) THEN
        RAISE EXCEPTION 'Cannot assign project: team % already has an assigned project.', p_team_id;
    END IF;

    IF p_start_date < v_team_created OR p_start_date > v_team_created + interval '7 days' THEN
        RAISE EXCEPTION 'Project start date must occur within 7 days of team creation date.';
    END IF;

    INSERT INTO teams.project_associations (team_id, project_id, team_manager_id, team_name, state, is_under_review)
    VALUES (p_team_id, p_project_id, v_manager_id, v_name, 1, false);

    INSERT INTO teams.project_association_details (team_id, project_name, start_date, end_date, state)
    VALUES (p_team_id, p_project_name, p_start_date, p_end_date, 0);

    UPDATE teams.teams SET last_activity_date = clock_timestamp() WHERE id = p_team_id;

    INSERT INTO teams.team_events (team_id, event_type, payload_json)
    VALUES (p_team_id, 'ProjectAssigned', jsonb_build_object('team_id', p_team_id, 'project_id', p_project_id));
END;
$$ LANGUAGE plpgsql;

-- 7.5 Suspend Project Detail
CREATE OR REPLACE FUNCTION teams.sp_team_suspendre_project_detail(
    p_team_id       uuid,
    p_project_name  varchar(150),
    p_manager_id    uuid
) RETURNS void AS $$
DECLARE
    v_manager_id uuid;
BEGIN
    SELECT team_manager_id INTO v_manager_id FROM teams.teams WHERE id = p_team_id;

    IF v_manager_id <> p_manager_id THEN
        RAISE EXCEPTION 'Permission denied: only the assigned team manager can suspend a project.';
    END IF;

    IF EXISTS (
        SELECT 1 FROM teams.project_association_details
        WHERE team_id = p_team_id AND project_name = p_project_name AND state = 0
          AND start_date > clock_timestamp()
    ) THEN
        RAISE EXCEPTION 'Cannot suspend project: project detail has not started yet.';
    END IF;

    UPDATE teams.project_association_details
    SET state = 1, suspended_at = clock_timestamp()
    WHERE team_id = p_team_id AND project_name = p_project_name AND state = 0;

    UPDATE teams.project_associations SET state = 2 WHERE team_id = p_team_id;

    INSERT INTO teams.team_events (team_id, event_type, payload_json)
    VALUES (p_team_id, 'ProjectSuspendedFromTeam', jsonb_build_object('team_id', p_team_id, 'project_name', p_project_name));
END;
$$ LANGUAGE plpgsql;

-- 7.6 Calculate Metrics
CREATE OR REPLACE FUNCTION teams.sp_team_calculer_indicateurs(
    p_team_id               uuid,
    p_average_productivity  double precision DEFAULT NULL
) RETURNS void AS $$
DECLARE
    v_turnover double precision;
BEGIN
    SELECT 100.0 * count(*) FILTER (WHERE leave_date >= clock_timestamp() - interval '2 months')
           / NULLIF(count(*), 0)
    INTO v_turnover
    FROM teams.team_members_history
    WHERE team_id = p_team_id;

    UPDATE teams.teams
    SET average_productivity = COALESCE(p_average_productivity, average_productivity),
        taux_turnover        = COALESCE(v_turnover, 0)
    WHERE id = p_team_id;

    IF p_average_productivity IS NOT NULL THEN
        INSERT INTO teams.team_productivity_history (team_id, productivity)
        VALUES (p_team_id, p_average_productivity);
    END IF;
END;
$$ LANGUAGE plpgsql;

-- 7.7 Soft Delete Team
CREATE OR REPLACE FUNCTION teams.sp_team_supprimer(
    p_team_id               uuid,
    p_requesting_user_role  varchar(20)
) RETURNS void AS $$
BEGIN
    IF p_requesting_user_role <> 'Admin' THEN
        RAISE EXCEPTION 'Permission denied: only administrators can delete teams.';
    END IF;

    IF EXISTS (
        SELECT 1 FROM teams.project_associations pa
        WHERE pa.team_id = p_team_id AND pa.state IN (1, 2, 3)
    ) THEN
        RAISE EXCEPTION 'Cannot delete team %: active project dependencies exist.', p_team_id;
    END IF;

    UPDATE teams.teams SET is_deleted = true WHERE id = p_team_id;

    INSERT INTO teams.team_events (team_id, event_type, payload_json)
    VALUES (p_team_id, 'TeamDeleted', jsonb_build_object('team_id', p_team_id));
END;
$$ LANGUAGE plpgsql;

/* ============================================================================
   8. SCHEDULED BACKGROUND JOBS
   ============================================================================ */

CREATE OR REPLACE FUNCTION teams.sp_job_archiver_equipes_inactives()
RETURNS void AS $$
BEGIN
    UPDATE teams.teams
    SET state = 2
    WHERE state <> 2 AND is_deleted = false
      AND last_activity_date < clock_timestamp() - interval '90 days';
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION teams.sp_job_detecter_turnover_eleve()
RETURNS TABLE(team_id uuid, turnover_pct double precision) AS $$
BEGIN
    RETURN QUERY
    SELECT h.team_id,
           100.0 * count(*) FILTER (WHERE h.leave_date >= clock_timestamp() - interval '2 months')
                 / NULLIF(count(*), 0)
    FROM teams.team_members_history h
    GROUP BY h.team_id
    HAVING 100.0 * count(*) FILTER (WHERE h.leave_date >= clock_timestamp() - interval '2 months')
                 / NULLIF(count(*), 0) > 50;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION teams.sp_job_detecter_chevauchement_equipes()
RETURNS TABLE(team_a uuid, team_b uuid, nb_membres_communs bigint, pct_chevauchement double precision) AS $$
BEGIN
    RETURN QUERY
    WITH effectifs AS (
        SELECT team_id, count(*) AS nb_membres FROM teams.team_members GROUP BY team_id
    )
    SELECT a.team_id, b.team_id, count(*),
           100.0 * count(*) / NULLIF(min(ea.nb_membres), 0)
    FROM teams.team_members a
    JOIN teams.team_members b ON b.member_id = a.member_id AND b.team_id > a.team_id
    JOIN effectifs ea ON ea.team_id = a.team_id
    JOIN effectifs eb ON eb.team_id = b.team_id
    GROUP BY a.team_id, b.team_id
    HAVING 100.0 * count(*) / NULLIF(min(ea.nb_membres), 0) > 50;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION teams.sp_job_detecter_instabilite_membre()
RETURNS TABLE(member_id uuid, nb_equipes_distinctes bigint, premiere_adhesion timestamptz, derniere_adhesion timestamptz) AS $$
BEGIN
    RETURN QUERY
    SELECT h.member_id, count(DISTINCT h.team_id), min(h.join_date), max(h.join_date)
    FROM teams.team_members_history h
    WHERE h.join_date >= clock_timestamp() - interval '30 days'
    GROUP BY h.member_id
    HAVING count(DISTINCT h.team_id) >= 3;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION teams.sp_job_detecter_productivite_faible()
RETURNS TABLE(team_id uuid, productivite_moyenne_3mois double precision) AS $$
BEGIN
    RETURN QUERY
    SELECT tph.team_id, avg(tph.productivity)
    FROM teams.team_productivity_history tph
    WHERE tph.measured_at >= clock_timestamp() - interval '3 months'
    GROUP BY tph.team_id
    HAVING avg(tph.productivity) < 40;
END;
$$ LANGUAGE plpgsql;

/* ============================================================================
   9. TRANSACTIONAL OUTBOX RELAY FUNCTIONS
   ============================================================================ */

-- 9.1 Claim Outbox Batch
CREATE OR REPLACE FUNCTION teams.fn_outbox_claim_batch(
    p_batch_size     integer  DEFAULT 50,
    p_claim_duration interval DEFAULT interval '30 seconds'
) RETURNS SETOF teams.team_events AS $$
BEGIN
    RETURN QUERY
    WITH claimed AS (
        SELECT id FROM teams.team_events
        WHERE processed = false
          AND (next_retry_at IS NULL OR next_retry_at <= clock_timestamp())
        ORDER BY occurred_on
        LIMIT p_batch_size
        FOR UPDATE SKIP LOCKED
    )
    UPDATE teams.team_events e
    SET next_retry_at = clock_timestamp() + p_claim_duration
    FROM claimed c
    WHERE e.id = c.id
    RETURNING e.*;
END;
$$ LANGUAGE plpgsql;

-- 9.2 Mark Events as Published
CREATE OR REPLACE FUNCTION teams.fn_outbox_mark_published(
    p_event_ids uuid[]
) RETURNS void AS $$
BEGIN
    UPDATE teams.team_events
    SET processed = true,
        processed_at = clock_timestamp(),
        next_retry_at = NULL
    WHERE id = ANY(p_event_ids);
END;
$$ LANGUAGE plpgsql;

-- 9.3 Handle Outbox Failure & Dead-Letter Threshold
CREATE OR REPLACE FUNCTION teams.fn_outbox_mark_failed(
    p_event_id  uuid,
    p_error     text,
    p_max_retry integer DEFAULT 5
) RETURNS void AS $$
DECLARE
    v_event teams.team_events%ROWTYPE;
BEGIN
    SELECT * INTO v_event FROM teams.team_events WHERE id = p_event_id FOR UPDATE;

    IF v_event.retry_count + 1 >= p_max_retry THEN
        -- Move to Dead Letter Table
        INSERT INTO teams.team_events_dead_letter (
            id, team_id, event_type, payload_json, exchange,
            routing_key, correlation_id, occurred_on, retry_count, last_error
        ) VALUES (
            v_event.id, v_event.team_id, v_event.event_type, v_event.payload_json, v_event.exchange,
            v_event.routing_key, v_event.correlation_id, v_event.occurred_on, v_event.retry_count + 1, p_error
        );

        DELETE FROM teams.team_events WHERE id = p_event_id;
    ELSE
        -- Exponential Backoff: 2^(retry_count+1) * 5 seconds
        UPDATE teams.team_events
        SET retry_count = retry_count + 1,
            last_error = p_error,
            next_retry_at = clock_timestamp() + (power(2, retry_count + 1) * interval '5 seconds')
        WHERE id = p_event_id;
    END IF;
END;
$$ LANGUAGE plpgsql;

/* ============================================================================
   10. STRATEGIC MONITORING & REPORTING VIEWS
   ============================================================================ */

-- Vue 1 : Synthetic Overview of Active Teams
CREATE OR REPLACE VIEW teams.vw_active_teams_overview AS
SELECT 
    t.id AS team_id,
    t.name AS team_name,
    t.team_manager_id,
    t.state,
    t.average_productivity,
    t.taux_turnover,
    COUNT(tm.member_id) AS member_count,
    pa.project_id,
    pa.state AS project_state,
    t.last_activity_date
FROM teams.teams t
LEFT JOIN teams.team_members tm ON tm.team_id = t.id
LEFT JOIN teams.project_associations pa ON pa.team_id = t.id
WHERE t.is_deleted = false AND t.state <> 2
GROUP BY t.id, pa.project_id, pa.state;

-- Vue 2 : Overlap Analysis (>50% member sharing)
CREATE OR REPLACE VIEW teams.vw_team_composition_overlaps AS
WITH team_sizes AS (
    SELECT team_id, COUNT(*) AS size FROM teams.team_members GROUP BY team_id
)
SELECT 
    m1.team_id AS team_a_id,
    m2.team_id AS team_b_id,
    COUNT(m1.member_id) AS shared_members,
    ROUND((100.0 * COUNT(m1.member_id) / LEAST(s1.size, s2.size))::numeric, 2) AS overlap_percentage
FROM teams.team_members m1
JOIN teams.team_members m2 ON m1.member_id = m2.member_id AND m1.team_id < m2.team_id
JOIN team_sizes s1 ON s1.team_id = m1.team_id
JOIN team_sizes s2 ON s2.team_id = m2.team_id
GROUP BY m1.team_id, m2.team_id, s1.size, s2.size
HAVING (100.0 * COUNT(m1.member_id) / LEAST(s1.size, s2.size)) > 50.0;

-- Vue 3 : Transactional Outbox Health Monitor
CREATE OR REPLACE VIEW teams.vw_outbox_health AS
SELECT 
    COUNT(*) FILTER (WHERE processed = false AND (next_retry_at IS NULL OR next_retry_at <= clock_timestamp())) AS pending_events,
    COUNT(*) FILTER (WHERE processed = false AND next_retry_at > clock_timestamp()) AS retry_scheduled_events,
    (SELECT COUNT(*) FROM teams.team_events_dead_letter) AS dead_letter_count,
    MAX(occurred_on) FILTER (WHERE processed = false) AS oldest_unprocessed_event
FROM teams.team_events;