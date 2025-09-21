job "api-team" {
  datacenters = ["dev-local-lamboft"]

  group "backend_api" {
    count = 1

    network {
      port "http" {
        to = 8181
      }
    }

    restart {
      attempts = 0
      mode     = "fail"
    }

    reschedule {
      attempts  = 0
      unlimited = false
    }

    service {
      provider = "nomad"
      name     = "team-management"
      port     = "http"

      check {
        type     = "http"
        path     = "/team-management/health"
        interval = "10s"
        timeout  = "2s"
      }
    }

    task "api-team" {
      driver = "docker"

      config {
        image = "${env.CI_REGISTRY}/${env.CI_PROJECT_NAMESPACE}/${env.CI_PROJECT_NAME}:${env.DOCKER_TAG}"
        ports = ["http"]
        volumes = [
          "./appsettings.json:/app/Teams/API.Layer/appsettings.Development.json:ro"
        ]
        cpu_hard_limit = true
      }

      env {
        ASPNETCORE_ENVIRONMENT                              = "Development"
        ASPNETCORE_URLS                                     = "https://+:8181"
        LOG_LEVEL                                           = "debug"
        ASPNETCORE_Kestrel__Certificates__Default__Password = "${env.CERT_PASSWORD}"
        ASPNETCORE_Kestrel__Certificates__Default__Path     = "/etc/ssl/certs/localhost.pfx"
      }

      resources {
        cpu    = 250
        memory = 250
      }
    }
  }
}
