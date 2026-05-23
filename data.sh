# //  {
# //         "Name": "Console",
# //         "Args": {
# //           "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"
# //         }
# //       },
# // uniquement pour la console

# faire un script pour la publication de message vers rabbitMQ


  #   // Pattern:          | GUID | TeamName/ProjectName
  #   // Project affected  | GUID | TeamName    (eg. Project affected  | b14db1e2-026e-4ac9-9739-378720de6f5b | Pentester)
  #   // Project suspended | GUID | ProjectName (eg. Project suspended | b14db1e2-026e-4ac9-9739-378720de6f5b | Test d’Intrusion Applicatif)
  #   // Member to add     | GUID | TeamName    (eg. Member to add     | 12345678-90ab-cdef-1234-567890abcdef | Pentester)
  #   // Member to delete  | GUID | TeamName    (eg. Member to delete  | 12345678-90ab-cdef-1234-567890abcdef | Pentester)
  #   // Member to add     | GUID | TeamName    (eg. Member to add     | 12345678-90ab-cdef-1234-567890abcdef | Security Architect)



  #  https://jsonbin.io/app/bins
  #   {
  #       "MemberTeamId": "12345678-90ab-cdef-1234-567890abcdef",
  #       "SourceTeam": "Equipe de sécurité (Security Team)",
  #       "DestinationTeam": "Pentester",
  #       "AffectationStatus": {
  #           "IsTransferAllowed": true,
  #           "ContratType": "CDI",
  #           "LeaveDate": "2025-07-03T12:34:56Z"
  #       }
  #   }
    
  #   {
  #       "ProjectId": "fa4c7e5b-c03b-4b5a-8f3f-d2bca6e6b2a0",
  #       "TeamManagerId": "b14db1e2-026e-4ac9-9739-378720de6f5b",
  #       "TeamName": "Pentester",
  #       "Details" : [
  #           {
  #              "ProjectName": "Tests de Phishing et d'Ingénierie Sociale",
  #              "ProjectStartDate": "2025-08-14T10:00:00Z",
  #              "ProjectEndDate": "2025-08-30T10:00:00Z",
  #               "ProjectState": {
  #                 "State": "Suspended"
  #               }
  #           }
  #       ]
  #   }
    
  #   A terme projet affecté
# {
#   "TeamManagerId": "b14db1e2-026e-4ac9-9739-378720de6f5b",
#   "TeamName": "Pentester",
#   "Details": [
    # {
    #   "ProjectName": "Tests de Phishing et Ingénierie Sociale",
    #   "ProjectStartDate": "2025-09-29T10:00:00Z",
    #   "ProjectEndDate": "2025-10-30T10:00:00Z",
    #   "VoState": {
    #     "State": "Active"
    #   }
    # },
    # {
    #   "ProjectName": "Audit de Sécurité Réseau",
    #   "ProjectStartDate": "2025-09-28T16:49:00Z",
    #   "ProjectEndDate": "2025-09-28T16:50:00Z",
    #   "VoState": {
    #     "State": "Active"
    #   }
    # },
#     {
#       "ProjectName": "Test d’Intrusion Applicatif",
#       "ProjectStartDate": "2025-09-26T09:00:00Z",
#       "ProjectEndDate": "2025-10-25T18:00:00Z",
#       "VoState": {
#         "State": "Active"
#       }
#     }
#   ]
# }

    
   

// Membre d'équipe
curl -X PUT \
  -H "Content-Type: application/json" \
  -H 'X-Master-Key: $2a$10$VmkEpsOPdIn2n1aYDzugA.SrQD2axB2g0usa1YLYETe9MIoUfqvjG' \
  -d '{
  "MemberTeamId": "12345678-90ab-cdef-1234-567890abcdef",
  "SourceTeam": "Equipe de sécurité (Security Team)",
  "DestinationTeam": "Pentester",
  "AffectationStatus": {
    "IsTransferAllowed": true,
    "ContratType": "Consultant",
    "LeaveDate": "2025-07-03T12:34:56Z"
  }
}' \
https://api.jsonbin.io/v3/b/68a8d6d3ae596e708fd193c0

// Projet
curl -X PUT \
  -H "Content-Type: application/json" \
  -H 'X-Master-Key: $2a$10$VmkEpsOPdIn2n1aYDzugA.SrQD2axB2g0usa1YLYETe9MIoUfqvjG' \
  -d '{
  "TeamManagerId": "b14db1e2-026e-4ac9-9739-378720de6f5b",
  "TeamName": "Pentester",
  "Details": [
    {
      "ProjectName": "Tests de Phishing et Ingénierie Sociale",
      "ProjectStartDate": "2025-09-02T10:00:00Z",
      "ProjectEndDate": "2025-09-30T10:00:00Z",
      "ProjectState": {
        "State": "Active"
      }
    }
  ]
}' \
https://api.jsonbin.io/v3/b/68a8d6d3ae596e708fd193c0


curl -X PUT \
  -H "Content-Type: application/json" \
  -H 'X-Master-Key: $2a$10$VmkEpsOPdIn2n1aYDzugA.SrQD2axB2g0usa1YLYETe9MIoUfqvjG' \
  -d '{
  "MemberTeamId": "12345678-90ab-cdef-1234-567890abcdef",
  "SourceTeam": "Equipe de sécurité (Security Team)",
  "DestinationTeam": "Security Architect",
  "AffectationStatus": {
    "IsTransferAllowed": true,
    "ContratType": "CDI",
    "LeaveDate": "2025-07-03T12:34:56Z"
  }
}' \
https: //api.jsonbin.io/v3/b/68a8d6d3ae596e708fd193c0

curl -X PUT \
  -H "Content-Type: application/json" \
  -H 'X-Master-Key: $2a$10$VmkEpsOPdIn2n1aYDzugA.SrQD2axB2g0usa1YLYETe9MIoUfqvjG' \
  -d '{
  "TeamManagerId": "a22b89a7-01ab-40d8-8904-b5f1ceadbd90",
  "TeamName": "Security Architect",
  "Details": [
    {
      "ProjectName": "Burp Suite Enterprise Implementation",
      "ProjectStartDate": "2025-08-28T10:00:00Z",
      "ProjectEndDate": "2025-09-30T10:00:00Z",
      "ProjectState": {
        "State": "Active"
      }
    }
  ]
}' \
https://api.jsonbin.io/v3/b/68a8d6d3ae596e708fd193c0

# Pour les tests
# dotnet test Teams.Tests/Teams.Tests.csproj \
#   --collect:"XPlat Code Coverage" \
#   --results-directory ./coverage

# ~/.dotnet/tools/reportgenerator \
#   -reports:./coverage/*/coverage.cobertura.xml \
#   -targetdir:./coverage-report \
#   -reporttypes:Html
