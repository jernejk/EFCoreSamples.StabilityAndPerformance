bombardier -c 5 -t 60s -d 60s -l https://localhost:5001/ExamplesCount/worstCase > Results/examples-count-worst-case.txt
Start-Sleep -s 10

bombardier -c 5 -t 60s -d 60s -l https://localhost:5001/ExamplesCount/worstCaseNoTracking > Results/examples-count-worst-case-no-tracking.txt
Start-Sleep -s 10

bombardier -c 5 -t 60s -d 60s -l https://localhost:5001/ExamplesCount/worstCaseNoTrackingIdOnly > Results/examples-count-worst-case-no-tracking-id-only.txt
Start-Sleep -s 10

bombardier -c 5 -t 60s -d 60s -l https://localhost:5001/ExamplesCount/bestCase > Results/examples-count-best-case.txt
Start-Sleep -s 10

bombardier -c 5 -t 60s -d 60s -l https://localhost:5001/ExamplesCount/rawSql > Results/examples-count-raw-sql.txt
Start-Sleep -s 10

bombardier -c 5 -t 60s -d 60s -l https://localhost:5001/ExamplesCount/rawSqlCommand > Results/examples-count-raw-sql-command.txt
Start-Sleep -s 10
