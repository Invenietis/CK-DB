-- SetupConfig: {}
--
-- Destroys a Culture, be it an actual culture (LCID) or a pure XLCID.
-- When destroying a LCID, all XLCID that have the LCID as their primary culture are also destroyed.
--
create procedure CK.sCultureDestroy
(
	@XLCID int
)
as
begin
	if @XLCID = 12 or @XLCID = 9 throw 50000, 'Res.EnglishAndFrenchNotDestroyable', 1;

	--[beginsp]

	--<PreDestroy revert />

	--! Removes main mapping for the culture.
	delete m from CK.tXLCIDMap m where m.XLCID = @XLCID;

	-- If it is an actual culture:
	if @XLCID < 0xFFFF -- LCID = 0xFFFF is invalid.
	begin
		
		--! Destroys XLCID that have this LCID as their primary LCID.
		declare @DelXLCID int;
		declare @CXLCID cursor;
		set @CXLCID = cursor local fast_forward for 
			select XLCID from CK.tXLCID where XLCID > 0x10000 and (XLCID&0xFFFF) = @XLCID;
		open @CXLCID
		fetch from @CXLCID into @DelXLCID
		while @@FETCH_STATUS = 0
		begin
			exec CK.sCultureDestroy @DelXLCID;
			fetch next from @CXLCID into @DelXLCID;
		end
		deallocate @CXLCID;

		--<PreDestroyLCID revert />

		-- !Removes all mapping from others.
		delete m from CK.tXLCIDMap m where m.LCID = @XLCID;
		-- Brutal update of Idx to fill the gap:
		update t set Idx = r.Idx
			from CK.tXLCIDMap t
			inner join (select XLCID, LCID, Idx = ROW_NUMBER() OVER(partition by m.XLCID ORDER BY m.Idx) - 1 from CK.tXLCIDMap m) r 
						on r.XLCID = t.XLCID and t.LCID = r.LCID;
		-- !Removes the LCID itself.
		delete c from CK.tLCID c where c.LCID = @XLCID;

		--<PostDestroyLCID />
	end
	--! Finally removes the XLCID itself.
	delete c from CK.tXLCID c where c.XLCID = @XLCID;

	--<PostDestroy />

	--[endsp]
end