-- SetupConfig: {}
create procedure CK.sResDestroy
	@ResId int
as
begin
	if @ResId <= 1 throw 50000, 'Res.Undestroyable', 1;

	--[beginsp]

	--<PreDestroy revert />

	delete from CK.tRes where ResId = @ResId;

	--<PostDestoy />

	--[endsp]
end
