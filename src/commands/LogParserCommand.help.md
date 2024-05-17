# cstone parse-logs command

## Command

`cstone parse-logs [-d|--directory {path}]  [-o|--output (raw|console|csv)]`

## Sample Output

|date               |playerId     |shopId       |shopName                            |kioskId      |kioskState|result|type|client_price|itemClassGUID                       |itemName                          |quantity|
|-------------------|-------------|-------------|------------------------------------|-------------|----------|------|----|------------|------------------------------------|----------------------------------|--------|
|2023-09-11 17.23.35|1069522220447|1171261755733|SCShop_RestStop_Pharmacy-001        |1171261755732|          |      |    |500         |7d50411f-088c-4c99-b85a-a6eaf95504c3|crlf_consumable_healing_01        |5       |
|2023-09-12 19.25.21|1069522220447|1158922531446|SCShop_ShubinInterstellar_NewBabbage|1158922531444|          |      |    |1000        |6a3daab4-c2c2-4a11-a7a6-a9c77baf6b1d|slaver_undersuit_01_01_01         |1       |
|2023-09-12 19.25.50|1069522220447|1158922531446|SCShop_ShubinInterstellar_NewBabbage|1158922531444|          |      |    |1000        |6a3daab4-c2c2-4a11-a7a6-a9c77baf6b1d|slaver_undersuit_01_01_01         |1       |
|2023-09-12 19.28.46|1069522220447|1158922531446|SCShop_ShubinInterstellar_NewBabbage|1158922531444|          |      |    |5015        |2b8d69d2-1aa9-4198-96af-5065ef3bda88|rrs_specialist_heavy_core_01_02_01|1       |
|2023-09-12 19.28.54|1069522220447|1158922531446|SCShop_ShubinInterstellar_NewBabbage|1158922531444|          |      |    |1311        |5da17f91-240e-4584-8c50-d3b38fa31c89|rrs_specialist_heavy_arms_01_02_01|1       |
|2023-09-12 19.29.09|1069522220447|1158922531446|SCShop_ShubinInterstellar_NewBabbage|1158922531444|          |      |    |2400        |9a5d5351-6336-4629-b068-850555c6d286|rrs_combat_heavy_backpack_01_02_01|1       |


## Parameters

### -d|--directory

Defines which directory to look for records.

### -o|--output

Defines the output format.

### -t|--type

> **TODO:** Filters the log entry type (f.ex. purchase, sell, etc.).

### -s|--start

> **TODO:** Defines which date time to start from.

### -e|--end

> **TODO:** Defines which date time to end at.

### -p|--player

> **TODO:** Defines which player id to filter.

### -w|--watch

> **TODO:** Watches the files for changes.

