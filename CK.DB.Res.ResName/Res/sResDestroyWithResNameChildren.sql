-- SetupConfig: { "Requires": [ "CK.sResDestroyResNameChildren" ] }
--
-- Destroys the resource and all its children according to their ResName.
-- Setting @ResNameOnly to 1 will only destroy the resource names, not the whole resources.
--
create procedure CK.sResDestroyWithResNameChildren
(
	@ResId int,
	@ResNameOnly bit = 0
)
as begin
	if @ResId <= 1 raiserror( 'Res.Undestroyable', 16, 1 );

	--[beginsp]

	exec CK.sResDestroyResNameChildren @ResId, @ResNameOnly;
	if @ResNameOnly = 1 exec CK.sResNameDestroy @ResId;
	else exec CK.sResDestroy @ResId;
	
	--[endsp]
end
