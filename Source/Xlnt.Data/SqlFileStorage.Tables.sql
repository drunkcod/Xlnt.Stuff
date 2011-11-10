if not exists(select null from sys.tables where name = 'SqlFileStorage.Blobs')
begin
	create table [SqlFileStorage.Blobs](
		Id int identity primary key,
		Data varbinary(max) not null)
end

if not exists(select null from sys.tables where name = 'SqlFileStorage.Files')
begin
	create table [SqlFileStorage.Files](
		Id int identity,
		Name varbinary(max) not null,
		Created datetime not null,
		Blob int foreign key references [SqlFileStorage.Blobs](Id))	
end
