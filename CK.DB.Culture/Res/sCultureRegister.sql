-- SetupConfig: {}
--
-- Updates or creates an actual Culture.
-- When creating a new culture, its fallbacks are by default the english ones
-- and this newcomer is added as the last fallback to any existing XLCID.
-- @Name, @EnglishName and @NativeName must not be empty nor contain comma (,) or pipe (|).
--
create procedure CK.sCultureRegister
(
	@LCID int,
	@Name varchar(20),
	@EnglishName varchar(50),
	@NativeName nvarchar(50)
)
as
begin
	if @LCID <= 0 or @LCID >= 0xFFFF throw 50000, 'Culture.LCIDMustBeBetween0And0xFFFF', 1;
	select @Name = rtrim(ltrim(@Name)), 
			@EnglishName = rtrim(ltrim(@EnglishName)), 
			@NativeName = rtrim(ltrim(@NativeName))
		where charIndex( @Name, ',' ) = 0 and charIndex( @Name, '|' ) = 0 
				and charIndex( @EnglishName, ',' ) = 0 and charIndex( @EnglishName, '|' ) = 0
				and charIndex( @NativeName, N',' ) = 0 and charIndex( @NativeName, N'|' ) = 0 ;
	if @Name is null or @EnglishName is null or @NativeName is null 
		throw 50000, 'Culture.InvalidCultureName', 1;

	--[beginsp]
	
	declare @NewOne bit = 0;
	declare @LCIDCount int;
	-- Current number of cultures (without the 0).
	select @LCIDCount = count(*)-1 from CK.tLCID;
	if not exists( select * from CK.tXLCID where XLCID = @LCID )
	begin
		set @NewOne = 1;
		insert into CK.tXLCID( XLCID ) values( @LCID );
	end
	merge CK.tLCID as target
		using ( select LCID = @LCID ) 
		as source on source.LCID = target.LCID
		when matched then update set Name = @Name, EnglishName = @EnglishName, NativeName = @NativeName
		when not matched by target then insert( LCID, Name, EnglishName, NativeName, XLCIDCount ) values( source.LCID, @Name, @EnglishName, @NativeName, 0 );
	if @NewOne = 1 
	begin
		insert into CK.tXLCIDMap( XLCID, Idx, LCID )
			-- Self-mapped entry.
			select @LCID, 0, @LCID
			union all
			-- The new LCID uses the English fallbacks by default.
			select @LCID, en.Idx+1, en.LCID
				from CK.tXLCIDMap en
				where en.XLCID = 9
			union all
			-- All other XLCID have the new one as their last fallback.
			select other.LCID, @LCIDCount, @LCID
				from CK.tLCID other
				where other.LCID > 0 and other.LCID <> @LCID;
	end
	--[endsp]
end
