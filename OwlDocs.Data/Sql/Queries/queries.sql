select * from OwlDocuments

delete from dbo.OwlDocuments 
	where id = 13

WITH docData AS (
  (SELECT id, ParentId, Path, ParentPath, Name, 1 AS level
  FROM OwlDocuments
  WHERE id = 1)
  UNION ALL
  (SELECT this.id, this.ParentId, this.Path, this.ParentPath, this.Name, prior.level + 1
  FROM docData prior
  INNER JOIN OwlDocuments this ON this.ParentId = prior.Id)
)
SELECT d.Id, d.ParentId, d.Path, d.ParentPath, d.Name, d.level
FROM docData d
ORDER BY d.level;