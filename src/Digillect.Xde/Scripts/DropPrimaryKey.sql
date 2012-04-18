
SET NOCOUNT ON

DECLARE @tname SYSNAME
SET @tname = 'XT_'+ '{0}'

--IF NOT EXISTS (SELECT * FROM SYSOBJECTS WHERE [NAME] = @tname) 
--	RETURN
	
IF NOT EXISTS (SELECT * FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_SCHEMA] = 'dbo' and [TABLE_NAME] = @tname)
	RETURN
	
DECLARE @pkname SYSNAME

/*SELECT
	@PKNAME = TSO.NAME
FROM
	SYSOBJECTS TSO 
		INNER JOIN SYSINDEXES TSI ON TSO.NAME = TSI.NAME
		INNER JOIN SYSOBJECTS TST ON TSI.ID = TST.ID
WHERE 
	OBJECTPROPERTY(TSO.ID, N'IsPrimaryKey') = 1 and TST.NAME = @tname*/
	
SELECT 
	@pkname = [CONSTRAINT_NAME] 
FROM 
	[INFORMATION_SCHEMA].[TABLE_CONSTRAINTS]
WHERE
	[TABLE_SCHEMA] = 'dbo' and [CONSTRAINT_TYPE] = 'PRIMARY KEY' and [TABLE_NAME] = @tname

IF @pkname IS NOT NULL
BEGIN
	DECLARE @cmd NVARCHAR(4000)
	SET @cmd = 'ALTER TABLE dbo.[' + @tname + '] DROP CONSTRAINT [' + @pkname + ']'
	EXEC (@cmd)
END
