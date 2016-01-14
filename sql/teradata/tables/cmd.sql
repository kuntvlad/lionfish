-- 1. seznam vsech tabulek, view a procedur - kvuli creatorname, coz je schema a kvuli rozeznani, co je Table, View a Procedure
SELECT '##DBNAME##', creatorname, tablename from dbc.tables WHERE TABLEKIND = 'T';

--split

SELECT '##DBNAME##', creatorname, tablename from dbc.tables WHERE TABLEKIND = 'V';

-- 2. vytahnout datove typy sloupcu
--split

SELECT
'##DBNAME##',
TABLENAME,
COLUMNNAME,
TRIM(COLUMNTYPE)||'('||TRIM(COLUMNNUM)||')'
FROM (
SELECT
DATABASENAME,
TABLENAME,
COLUMNNAME,
CASE WHEN COLUMNTYPE='CF' THEN 'CHAR'
     WHEN COLUMNTYPE='CV' THEN 'VARCHAR'
     WHEN COLUMNTYPE='D' THEN 'DECIMAL' 
     WHEN COLUMNTYPE='TS' THEN 'TIMESTAMP'      
     WHEN COLUMNTYPE='I' THEN 'INTEGER'
     WHEN COLUMNTYPE='I2' THEN 'SMALLINT'
     WHEN COLUMNTYPE='DA' THEN 'DATE'
END AS COLUMNTYPE,
CASE WHEN COLUMNTYPE='CF' THEN COLUMNLENGTH
     WHEN COLUMNTYPE='CV' THEN COLUMNLENGTH
     WHEN COLUMNTYPE='D' THEN (DECIMALTOTALDIGITS||','||DECIMALFRACTIONALDIGITS)
     WHEN COLUMNTYPE='TS' THEN COLUMNLENGTH      
     WHEN COLUMNTYPE='I' THEN DECIMALTOTALDIGITS
     WHEN COLUMNTYPE='I2' THEN DECIMALTOTALDIGITS
     WHEN COLUMNTYPE='DA' THEN NULL
END AS COLUMNNUM
FROM DBC.COLUMNS
WHERE DATABASENAME='##DBNAME##' ) TBL;

-- 3. ziskat zdrojove kody - zavolat na vsechno SHOW prikaz (vraci jeden sloupec, jeden radek)
--split

SHOW TABLE '##DBNAME##'.'##TABLENAME##';

--split

SHOW VIEW '##DBNAME##'.'##VIEWNAME##';

