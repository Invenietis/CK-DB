-- SetupConfig: {}
--
-- Sets a string value for a resource in a given culture. 
-- When Value is null, it is removed.
-- This can be called with an actual culture (LCID) or a XLCID: the low word (primary LCID) is used.
--
create procedure CK.sMCResStringSet
(
	@ResId int,
	@LCID int,
	@Value nvarchar(400)
)
as
begin
	set nocount on;
	if @ResId <= 0 throw 50000, 'Res.InvalidResId', 1;
	if @LCID <= 0 throw 50000, 'Culture.InvalidLCID', 1;
	merge CK.tMCResString as target
		using ( select ResId = @ResId, LCID = (@LCID & 0xFFFF) ) 
		as source on source.ResId = target.ResId and source.LCID = target.LCID
		when matched and @Value is null then delete
		when matched then update set Value = @Value
		when not matched by target and @Value is not null then insert( ResId, LCID, Value ) values( source.ResId, source.LCID, @Value );	
	return 0;
end