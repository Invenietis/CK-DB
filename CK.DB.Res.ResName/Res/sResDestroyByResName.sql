-- SetupConfig: { "Requires": [ "CK.sResDestroy", "CK.sResNameDestroy" ] }
--
-- Destroys a root resource and/or its children thanks to its name.
-- Note that if @WithRoot and @WithChildren are both 0, nothing is done.
-- If the root name doesn't exist, its children can nevertheless be destroyed.
-- Setting @ResNameOnly to 0 will call CK.sResDestroy, destroying the ResId and all its resources.
-- By default, only the resource name is destroyed (this is the safest way).
-- 
create procedure CK.sResDestroyByResName
(
	@RootResName varchar(128),
	@WithRoot bit = 1,
	@WithChildren bit = 1,
	@ResNameOnly bit = 1
)
as begin
    --[beginsp]

    --<PresResDestroyByResName revert />
    declare @RootResId int;

    if @WithRoot = 1 
    begin
	    select @RootResId = r.ResId 
		    from CK.tResName r
		    where r.ResName = @RootResName;
		if @ResNameOnly = 1 exec CK.sResNameDestroy @RootResId;
		else exec CK.sResDestroy @RootResId;
    end
    if @WithChildren = 1
	begin
        declare @ChildResId int;
	    declare @CRes cursor;
	    set @CRes = cursor local fast_forward for 
		    select r.ResId 
			    from CK.tResName r
			    where r.ResName like @RootResName + '.%';
	    open @CRes;
	    fetch from @CRes into @ChildResId;
	    while @@FETCH_STATUS = 0
	    begin
		    if @ResNameOnly = 1 exec CK.sResNameDestroy @ChildResId;
		    else exec CK.sResDestroy @ChildResId;
		    fetch next from @CRes into @ChildResId;
	    end
	    deallocate @CRes;
    end

	--<PostsResDestroyByResName />

    --[endsp]
end

