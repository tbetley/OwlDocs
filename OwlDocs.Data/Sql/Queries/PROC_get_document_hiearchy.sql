CREATE PROCEDURE dbo.get_document_hiearchy

as

WITH docData AS (
  (SELECT Id, ParentId, Path, ParentPath, Name, Markdown, Html, Type, Data, 1 AS level
  FROM Documents
  WHERE id = 1)
  UNION ALL
  (SELECT 
	this.Id, 
	this.ParentId, 
	this.Path, 
	this.ParentPath, 
	this.Name, 
	this.Markdown,
	this.Html,
	this.Type,
	this.Data,
	prior.level + 1
  FROM docData prior
  INNER JOIN Documents this ON this.ParentId = prior.Id)
)
SELECT 
	d.Id, 
	d.ParentId, 
	d.Path,
	d.ParentPath, 
	d.Name,
	d.Markdown,
	d.Html,
	d.Type,
	d.Data
FROM docData d
ORDER BY d.level;

