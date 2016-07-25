-- SetupConfig: {}
--
-- Destroys a Culture. There must not be any specific culture: specific cultures must be destroyed first.
--
create procedure CK.sCultureDestroy
(
	@LCID int
)
as
begin
	if @LCID <= 0 or @LCID = 127 or @LCID > 0xFFFF throw 50000, 'Res.LCIDMustBeBetween0And0xFFFFAndNot127', 1;
	if @LCID = 12 or @LCID = 9 throw 50000, 'Res.EnglishAndFrenchNotDestroyable', 1;

	--[beginsp]

	--<PreDestroy revert />

	-- Removes main mapping for the culture.
	delete m from CK.tXLCIDMap m where m.XLCID = @LCID;
	-- Removes all mapping from others.
	delete m from CK.tXLCIDMap m where m.LCID = @LCID;

	-- Removes the culture...
	delete c from CK.tLCID c where c.LCID = @LCID;
	-- ...and its XLCID
	delete c from CK.tXLCID c where c.XLCID = @LCID;

	--<PostDestoy />

	--[endsp]
end