-- SetupConfig: {}
-- Version = 1.0.0
--
-- Renames a resource.
--
create procedure CK.sResNameRename
(
    @ResId int,
    @NewName varchar(128),
	@WithChildren bit = 1
)
as begin

	if @ResId <= 1 throw 50000, 'Res.NoRename', 1;
	set @NewName = RTrim( LTrim(@NewName) );
	
	--[beginsp]

	declare @OldName varchar(128);
	declare @LenPrefix int;
	select @OldName = ResName, 
		   @LenPrefix = len(ResName)+1
		from CK.tResName 
		where ResId = @ResId;

	if @OldName is not null 
	begin

		--<PreRename revert />

		if @WithChildren = 1
		begin
			-- Updates child names first.
			update CK.tResName set ResName = @NewName + substring( ResName, @LenPrefix, 128 )
				where ResName like @OldName + '.%';
		end
		-- Updates the resource itself.
		update CK.tResName set ResName = @NewName where ResId = @ResId;

		--<PostRename />
	
	end

	--[endsp]
end
