/* Select synonyms to export                                                       */
SELECT
	'##DBNAME##' as mydatabase,
	sch.name as myschema,
	s.name as name,
	(CASE WHEN (len(s.base_object_name) - len(REPLACE(s.base_object_name, '.', ''))) = 3
			THEN SUBSTRING(s.base_object_name,
			CHARINDEX('.', s.base_object_name, CHARINDEX('.', s.base_object_name) + 1) + 1,
			CHARINDEX('.', s.base_object_name, CHARINDEX('.', s.base_object_name, CHARINDEX('.', s.base_object_name) + 1) + 1) - CHARINDEX('.', s.base_object_name, CHARINDEX('.', s.base_object_name) + 1) - 1)
		  WHEN (len(s.base_object_name) - len(REPLACE(s.base_object_name, '.', ''))) = 2
			THEN SUBSTRING(s.base_object_name,
			CHARINDEX('.', s.base_object_name) + 1,
			CHARINDEX('.', s.base_object_name, CHARINDEX('.', s.base_object_name) + 1) - CHARINDEX('.', s.base_object_name) - 1)
		  WHEN (len(s.base_object_name) - len(REPLACE(s.base_object_name, '.', ''))) = 1
			THEN SUBSTRING(s.base_object_name,
							  1,
							 CHARINDEX('.', s.base_object_name) - 1)
		  ELSE ''
   END) as sourceSchema,
   (CASE WHEN (len(s.base_object_name) - len(REPLACE(s.base_object_name, '.', ''))) > 0
		  THEN REVERSE(SUBSTRING(REVERSE(s.base_object_name),
							 1,
							 CHARINDEX('.', REVERSE(s.base_object_name)) - 1))
		  ELSE s.base_object_name
  END) as sourceName,
  (CASE WHEN (len(s.base_object_name) - len(REPLACE(s.base_object_name, '.', ''))) = 3
				  THEN SUBSTRING(s.base_object_name, 1, CHARINDEX('.', s.base_object_name, CHARINDEX('.', s.base_object_name) + 1) - 1)
		WHEN (len(s.base_object_name) - len(REPLACE(s.base_object_name, '.', ''))) = 2
				  THEN SUBSTRING(s.base_object_name, 1, CHARINDEX('.', s.base_object_name) - 1)
		ELSE ''
  END) as sourceDbLinkName
FROM
	[##DBNAME##].sys.synonyms s
	INNER JOIN
	[##DBNAME##].sys.schemas sch on s.schema_id = sch.schema_id
/* Uncomment following line to filter out synonyms:                                */
/* WHERE name LIKE '%MyPattern%'                                                   */
;
