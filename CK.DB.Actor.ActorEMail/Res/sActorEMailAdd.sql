-- SetupConfig: { }
--
-- Adds an email to a user or a group and/or sets whether it is the primary one.
-- Optionally sets the ValTime to sysutcdatetime() or '0001-01-01'.
create procedure CK.sActorEMailAdd 
(
	@ActorId int,
	@UserOrGroupId int,
	@EMail nvarchar(255),
	@IsPrimary bit,
	@Validate bit = null
)
as
begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
	if @EMail is null throw 50000, 'Argument.NullEMail', 1;
	set @EMail = rtrim(ltrim(@EMail));
	if len(@EMail) = 0 throw 50000, 'Argument.EmptyEMail', 1;
	if @IsPrimary is null throw 50000, 'Argument.NullIsPrimary', 1;

	--[beginsp]
	declare @ExistingPrimaryEMail nvarchar(255);
	select @ExistingPrimaryEMail = EMail from CK.tActorEMail where ActorId = @UserOrGroupId and IsPrimary = 1
	declare @NewPrimaryEMail nvarchar(255);
	if @ExistingPrimaryEMail is null set @IsPrimary = 1;
	if @IsPrimary = 1 set @NewPrimaryEMail = @EMail;
	else set @NewPrimaryEMail = @ExistingPrimaryEMail;

	--<PreEMailAdd revert />

	-- Update or insert the @EMail.
	merge CK.tActorEMail as target
		using( select ActorId = @UserOrGroupId, EMail = @EMail ) 
		as source on source.ActorId = target.ActorId and source.EMail = target.EMail
		when matched then update set IsPrimary = @IsPrimary, 
									 ValTime = case when @Validate is null then target.ValTime 
													when @Validate = 0 then '0001-01-0'
													else sysutcdatetime() 
												end
		when not matched by target then insert( ActorId, EMail, IsPrimary, ValTime ) 
											values( @UserOrGroupId, 
													@EMail, 
													@IsPrimary, 
													case when @Validate is null or @Validate = 0 
														then '0001-01-01'
														else sysutcdatetime() 
													end );
	-- A little bit of defensive programming here: 
	-- we always reset the IsPrimary bit based on @NewPrimaryEMail or elect a new one.
	if @NewPrimaryEMail is null
	begin
		select top 1 @NewPrimaryEMail = EMail from CK.tActorEMail where ActorId = @UserOrGroupId order by ValTime desc;
	end

	if @NewPrimaryEMail is not null
	begin
		update CK.tActorEMail 
			set IsPrimary = case when EMail = @NewPrimaryEMail then 1 else 0 end 
			where ActorId = @UserOrGroupId;
	end

	--<PostEMailAdd />
	
	--[endsp]
end

