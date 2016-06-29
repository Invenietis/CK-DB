-- SetupConfig: {}
create procedure CK.sResCreate
	@ResId int output
as
begin

	--[beginsp]

	--<PreCreate revert />

	insert into CK.tRes default values;
	set @ResId = Scope_identity();

	--<PostCreate />

	--[endsp]
end
