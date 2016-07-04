-- SetupConfig:{}
--
-- Associates a name to an existing resource identifier.
--
create procedure CK.sResNameCreate
(
	@ResId int,
	@ResName varchar(128)
)
as 
begin
	set @ResName = RTrim( LTrim(@ResName) );

	--[beginsp]

	--<PreCreate revert />

	insert into CK.tResName( ResId, ResName ) values( @ResId, @ResName );
	
	--<PostCreate />	
	
	--[endsp]
end