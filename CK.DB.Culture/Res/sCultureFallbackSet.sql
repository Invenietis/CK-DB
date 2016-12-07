--
-- SetupConfig:{}
--
-- Sets the fallback of a LCID.
-- The @FallbacksLCID is a comma separated list of LCID (just like the ones
-- given by CK.vXLCID.FallbacksLCID.
-- The list must start with the @LCID itself otherwise an error is raised.
-- The list do not not need to be complete: existing LCID with their current ordering
-- will be automatically added.
--
alter procedure CK.sCultureFallbackSet
(
	@LCID int, 
	@FallbacksLCID varchar(max)
)
as
begin
	if @LCID <= 0 or @LCID > 0xFFFF throw 50000, 'Culture.InvalidLCID', 1;
	declare @Fallbacks table( Idx int not null identity(0,1), LCID int not null);
	declare @xml xml = '<t>' + REPLACE( @FallbacksLCID, ',', '</t><t>') + '</t>';
	-- No distinct here since this will sort the identifiers...
	-- Duplicates will result in a merge error: The MERGE statement attempted to UPDATE or DELETE the same row more than once.
	insert into @Fallbacks(LCID) select r.value('.','int') from @xml.nodes('/t') as records(r);
	if not exists( select * from @Fallbacks where LCID = @LCID and Idx = 0 )
		throw 50000, 'Culture.FallbackMustStartWithLCID', 1;

	--[beginsp]
	-- Appends any missing cultures based on existing fallbacks.
	insert into @Fallbacks( LCID )
		select m.LCID
				from CK.tXLCIDMap m
				where m.XLCID = @LCID and m.LCID not in (select LCID from @Fallbacks) 
				order by m.Idx;

	-- Replaces falbacks.
	merge CK.tXLCIDMap as target
		using( select Idx, LCID from @Fallbacks ) as source 
		on target.XLCID = @LCID and target.LCID = source.LCID
		when matched then update set Idx = source.Idx
		when not matched then insert( XLCID, Idx, LCID ) values( @LCID, source.Idx, source.LCID );

	--[endsp]
end

