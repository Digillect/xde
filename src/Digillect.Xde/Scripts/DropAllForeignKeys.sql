
SET NOCOUNT ON

DECLARE @tname SYSNAME
SET @tname = 'XT_'+ '{0}'

IF NOT EXISTS (SELECT * FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_SCHEMA] = 'dbo' and [TABLE_NAME] = @tname)
	RETURN

DECLARE @cname SYSNAME
DECLARE @cmd NVARCHAR(4000)

DECLARE c CURSOR FOR 
SELECT 
	[CONSTRAINT_NAME]
FROM 
	[INFORMATION_SCHEMA].[TABLE_CONSTRAINTS]
WHERE
	[TABLE_SCHEMA] = 'dbo' and [TABLE_NAME] = @tname and [CONSTRAINT_TYPE] = 'FOREIGN KEY'

OPEN c
FETCH NEXT FROM c INTO @CNAME
WHILE @@FETCH_STATUS = 0
BEGIN
	SET @cmd = 'ALTER TABLE dbo.[' + @TNAME + '] DROP CONSTRAINT [' + @cname + ']'
	EXEC(@cmd)
	FETCH NEXT FROM c INTO @CNAME
END
CLOSE c
DEALLOCATE c
	
	