-- SetupConfig: { "Requires": [ "CK.sResDestroy" ] }
--
-- Destroys all ressources which ResName start with @ResNamePrefix + '.'.
-- Setting @ResNameOnly to 0 will destroy the whole resources.
--
create procedure CK.sResDestroyByResNamePrefix
(
	@ResNamePrefix varchar(128),
	@ResNameOnly bit = 1
)
as begin
	--[beginsp]

	--<PreDestroyByResNamePrefix revert />

	declare @ChildResId int;
	declare @CRes cursor;
	set @CRes = cursor local fast_forward for 
		select r.ResId 
			from CK.tResName r
			where r.ResName like @ResNamePrefix + '.%';
	open @CRes;
	fetch from @CRes into @ChildResId;
	while @@FETCH_STATUS = 0
	begin
		if @ResNameOnly = 1 exec CK.sResNameDestroy @ChildResId;
		else exec CK.sResDestroy @ChildResId;
		fetch next from @CRes into @ChildResId;
	end
	deallocate @CRes;

	--<PostDestroyByResNamePrefix />

	--[endsp]
end
