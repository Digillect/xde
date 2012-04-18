
DECLARE @tableXdeName NVARCHAR(500)
SET @tableXdeName = '{0}'
SET NOCOUNT ON

IF NOT EXISTS (SELECT * FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_SCHEMA] = 'dbo' and [TABLE_NAME] = 'XT_' + @tableXdeName) 
	RETURN

CREATE TABLE #p (index_name sysname, index_description varchar(210), index_keys nvarchar(2078))

DECLARE @cname NVARCHAR(500)
DECLARE @tname NVARCHAR(500)
DECLARE @cmd NVARCHAR(4000)

set @tname = 'XT_'+ @tableXdeName

insert into #p exec sp_helpindex @TNAME

DECLARE @t TABLE (CNAME nvarchar(500), TNAME nvarchar(500))

INSERT INTO @t
SELECT 
	TSI.NAME, TSO.NAME 
FROM SYSINDEXES TSI
	INNER JOIN SYSOBJECTS TSO ON TSI.ID = TSO.ID
	INNER JOIN #p TSA ON TSI.NAME = TSA.index_name
	LEFT JOIN SYSOBJECTS TSN ON TSI.NAME = TSN.NAME
WHERE
	TSO.NAME = @tname AND TSN.NAME IS NULL

drop table #p
--select * from @t

DECLARE c CURSOR FOR SELECT CNAME, TNAME from @t
OPEN c
FETCH NEXT FROM c INTO @cname, @tname
WHILE @@FETCH_STATUS = 0
BEGIN
	SET @cmd = 'DROP INDEX dbo.['+ @tname + '].[' + @cname + ']'
	EXEC(@cmd)
	FETCH NEXT FROM c INTO @cname, @tname
END
CLOSE c
DEALLOCATE c

