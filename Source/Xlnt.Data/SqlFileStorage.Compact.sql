update [SqlFileStorage.Files]
set Blob = UniqueId
from [SqlFileStorage.Files] as Files
inner join (
	select Blob = Blobs.Id, UniqueId = UniqueBlobs.Id
	from (
		select Id = min(Id), Data
		from [SqlFileStorage.Blobs]
		group by [Data]) UniqueBlobs
	inner join [SqlFileStorage.Blobs] Blobs on Blobs.Data = UniqueBlobs.Data
) BlobIds on BlobIds.Blob = Files.Blob
where Files.Blob <> UniqueId

delete [SqlFileStorage.Blobs]
where Id not in(select Blob from [SqlFileStorage.Files])