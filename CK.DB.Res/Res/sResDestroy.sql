-- SetupConfig: {}
create procedure CK.sResDestroy
	@ResId int
as
begin
	
	--[beginsp]

	--<PreDestroy revert />

	delete from CK.tRes where ResId = @ResId;

	--<PostDestoy />

	--[endsp]
end
