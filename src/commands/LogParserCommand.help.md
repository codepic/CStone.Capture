# cstone parse-logs command

## Command

`cstone parse-logs [-d|--directory {path}]  [-o|--output (raw|console|csv)]`


## Parameters

### -p|--path

Defines which file or directory to look for records.

### -w|--watch

Watches the selected **file** for changes. See `-p|--path`.

### -o|--output

Defines the output format.

#### UIF

`cstone parse-logs -o uif`

Will output purchases in UIF-compatible format:

```csv
5ab6897b-81d3-4ce9-83f1-6b41cdee510b,,,30,,,2953-09-12,,,1158922531446
963a2e3b-7a40-4678-83a9-3638a72e177a,,,387,,,2953-09-12,,,1158922531446
63d5c4b1-7a41-42fc-b202-ea9b07fc6389,,,777,,,2953-09-12,,,1158922531446
9f8cbca6-d5be-4d2c-8bb4-0337410f98b0,,,534,,,2953-09-12,,,1158922531446
5ab6897b-81d3-4ce9-83f1-6b41cdee510b,,,30,,,,2953-09-12,,,1158922531446
963a2e3b-7a40-4678-83a9-3638a72e177a,,,387,,,,2953-09-12,,,1158922531446
63d5c4b1-7a41-42fc-b202-ea9b07fc6389,,,777,,,,2953-09-12,,,1158922531446
9f8cbca6-d5be-4d2c-8bb4-0337410f98b0,,,534,,,,2953-09-12,,,1158922531446
9de3a559-0136-4baf-b2ce-ab7fdda4bec5,,,387,,,,2953-09-12,,,1158922531446
5ab6897b-81d3-4ce9-83f1-6b41cdee510b,,,30,,,,2953-08-25,,,1109348376646
396ccb0d-c251-484d-998e-cc3616a37ee5,,,145,,,,2953-08-27,,,1071120277524
396ccb0d-c251-484d-998e-cc3616a37ee5,,,145,,,,2953-08-27,,,1071120277524
410cc514-6a00-441a-8128-ac11797f844a,,,183,,,,2953-08-27,,,1071120277524
5ab6897b-81d3-4ce9-83f1-6b41cdee510b,,,30,,,,2953-08-27,,,1109586376765
90361b62-f3ae-4a7d-9b57-ae2fd8addf8f,,,387,,,,2953-08-27,,,1109586376765
0907f3f9-ef08-446f-8a0c-003876d53bf9,,,387,,,,2953-08-27,,,1109586376765
b07c855e-79ac-4c5c-b7b7-aa8df9b1a987,,,684,,,,2953-08-27,,,1109586376765
b07c855e-79ac-4c5c-b7b7-aa8df9b1a987,,,684,,,,2953-08-27,,,1109586376765
```

#### CSV

|date               |playerId     |shopId       |shopName                            |kioskId      |kioskState|result|type|client_price|itemClassGUID                       |itemName                          |quantity|
|-------------------|-------------|-------------|------------------------------------|-------------|----------|------|----|------------|------------------------------------|----------------------------------|--------|
|2023-09-11 17.23.35|1069522220447|1171261755733|SCShop_RestStop_Pharmacy-001        |1171261755732|          |      |    |500         |7d50411f-088c-4c99-b85a-a6eaf95504c3|crlf_consumable_healing_01        |5       |
|2023-09-12 19.25.21|1069522220447|1158922531446|SCShop_ShubinInterstellar_NewBabbage|1158922531444|          |      |    |1000        |6a3daab4-c2c2-4a11-a7a6-a9c77baf6b1d|slaver_undersuit_01_01_01         |1       |
|2023-09-12 19.25.50|1069522220447|1158922531446|SCShop_ShubinInterstellar_NewBabbage|1158922531444|          |      |    |1000        |6a3daab4-c2c2-4a11-a7a6-a9c77baf6b1d|slaver_undersuit_01_01_01         |1       |
|2023-09-12 19.28.46|1069522220447|1158922531446|SCShop_ShubinInterstellar_NewBabbage|1158922531444|          |      |    |5015        |2b8d69d2-1aa9-4198-96af-5065ef3bda88|rrs_specialist_heavy_core_01_02_01|1       |
|2023-09-12 19.28.54|1069522220447|1158922531446|SCShop_ShubinInterstellar_NewBabbage|1158922531444|          |      |    |1311        |5da17f91-240e-4584-8c50-d3b38fa31c89|rrs_specialist_heavy_arms_01_02_01|1       |
|2023-09-12 19.29.09|1069522220447|1158922531446|SCShop_ShubinInterstellar_NewBabbage|1158922531444|          |      |    |2400        |9a5d5351-6336-4629-b068-850555c6d286|rrs_combat_heavy_backpack_01_02_01|1       |


### -t|--type

> **TODO:** Filters the log entry type (f.ex. purchase, sell, etc.).

### -s|--start

> **TODO:** Defines which date time to start from.

### -e|--end

> **TODO:** Defines which date time to end at.

### -p|--player

> **TODO:** Defines which player id to filter.


