begin tran
	insert [SqlFileStorage.Blobs](Data)
	values(@Data)

	insert [SqlFileStorage.Files](Name, Created, Blob)
	values(@Name, @Created, @@IDENTITY)
commit
