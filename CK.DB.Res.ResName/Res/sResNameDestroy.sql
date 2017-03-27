-- SetupConfig: {}
--
-- Destroys the resource name if it exists.
--
create procedure CK.sResNameDestroy
(
	@ResId int
)
as 
begin
	if @ResId <= 1 throw 50000, 'Res.Undestroyable', 1;

	--[beginsp]

	--<PreDestroy revert />

	delete from CK.tResName where ResId = @ResId;
	
	--<PostDestroy />	
	
	--[endsp]
end