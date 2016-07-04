-- SetupConfig: { "Requires": [ "CK.sResDestroy" ] }
--
-- Destroys all children of a given ressource according to their ResName.
-- Setting @ResNameOnly to 1 will only destroy the resource names, not the whole resources.
--
create procedure CK.sResDestroyResNameChildren
(
	@ResId int,
	@ResNameOnly bit = 0
)
as begin
	--[beginsp]

	--<PreDestroyChidren revert />

	declare @ChildResId int;
	declare @CRes cursor;
	set @CRes = cursor local fast_forward for 
		select c.ResId 
			from CK.tResName r
			inner join CK.tResName c on c.ResName like r.ResName + '.%'
			where r.ResId = @ResId;
	open @CRes;
	fetch from @CRes into @ChildResId;
	while @@FETCH_STATUS = 0
	begin
		if @ResNameOnly = 1 exec CK.sResNameDestroy @ChildResId;
		else exec CK.sResDestroy @ChildResId;
		fetch next from @CRes into @ChildResId;
	end
	deallocate @CRes;

	--<PostDestroyChidren />

	--[endsp]
end
