-- SetupConfig: {}
-- Sets the user name. 
-- There is no guaranty that the actual value will be the same as the one requested (if auto numbering 
-- is injected for example). 
--
create procedure CK.sUserUserNameSet
(
    @ActorId int,
    @UserId int,
    @UserName nvarchar(127),
	@Done bit output
)
as begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;

	--[beginsp]

	set @Done = 1;
	if exists( select * from CK.tUser where UserName = @UserName and UserId <> @UserId )
	begin
		set @Done = 0;
		--<UserNameSetClash />
	end
	if @Done = 1
	begin
		--<PreUserNameSet revert />

		update u 
			set u.UserName = @UserName
			from CK.tUser u   
			where u.UserId = @UserId;

		--<PostUserNameSet />
	end

	--[endsp]
end