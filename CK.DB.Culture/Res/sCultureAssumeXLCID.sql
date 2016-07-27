-- SetupConfig:{ "Requires": ["CK.vXLCID"] }
--
-- Finds or creates a XLCID with a specified fallbacks chain.
-- The @FallbacksLCID is a comma separated list of LCID (just like the ones
-- given by CK.vXLCID.FallbacksLCID.
-- The list do not not need to be complete (the smaller it is, the more chances there are
-- to reuse an existing fallback instead of creating a new one): LCID with their current ordering
-- will be automatically added based on the PrimaryLCID fallbacks.
--
-- When @AllowLCIDMapping is 1, a LCID may be returned if its current fallbacks satisfy the request.
-- Since fallbacks of a LCID can be changed, this is not guaranteed to be stable.
-- By default, a pure XLCID is obtained or created: it is immutable (as long as registered
-- cultures do not change) and will be destroyed only if its primary LCID is destroyed.
-- 
alter procedure CK.sCultureAssumeXLCID
(
	@FallbacksLCID varchar(max),
	@AllowLCIDMapping bit = 0,
	@XLCID int output 
)
as
begin
	set nocount on;
	declare @Fallbacks table( Idx int not null identity(0,1), LCID int not null);
	declare @xml xml = '<t>' + REPLACE( @FallbacksLCID, ',', '</t><t>') + '</t>'
	-- No distinct here since this will sort the identifiers...
	insert into @Fallbacks(LCID) select r.value('.','int') from @xml.nodes('/t') as records(r);

	--[beginsp]

	--! Checks that the primary LCID exists.
	declare @PrimaryLCID int;
	select @PrimaryLCID = f.LCID
		from @Fallbacks f
		inner join CK.tLCID l on l.LCID = f.LCID
		where f.Idx = 0;
	if @PrimaryLCID is null throw 50000, 'Culture.InvalidPrimaryLCID', 1;

	--! Tries to find an existing XLCID (the smallest/oldest one) that matches the fallbacks.
	select top 1 @XLCID = m.XLCID 
		from CK.vXLCID m 
		where (@AllowLCIDMapping = 1 or m.XLCID > 0x10000)
				and m.PrimaryLCID = @PrimaryLCID 
				and (m.FallbacksLCID = @FallbacksLCID or m.FallbacksLCID like @FallbacksLCID+',%')
		order by m.XLCID;
	if @@RowCount = 0
	begin
		--! Ensures that all the fallbacks LCID exist.
		if exists( select * from @Fallbacks where LCID not in (select LCID from CK.tLCID where LCID > 0) )
			throw 50000, 'Culture.InvalidLCIDFallback', 1;
		--! Appends any missing LCID based on fallbacks of the PrimaryLCID.
		insert into @Fallbacks( LCID )
			select m.LCID
					from CK.tXLCIDMap m
					where m.XLCID = @PrimaryLCID and m.LCID not in (select LCID from @Fallbacks) 
					order by m.Idx;
		--! Updates and gets the XLCIDCount.
		declare @XLCIDCount smallint;
		update CK.tLCID set @XLCIDCount = XLCIDCount = XLCIDCount+1 where LCID = @PrimaryLCID; 
		--! Computes the new XLCID.
		set @XLCID = cast(@XLCIDCount as int)*0x10000 + @PrimaryLCID;
		--! Inserts the new XLCID and its fallbacks.
		insert into CK.tXLCID( XLCID ) values( @XLCID );
		insert into CK.tXLCIDMap( XLCID, Idx, LCID ) 
			select @XLCID, Idx, LCID from @Fallbacks;
	end
	--[endsp]
end

