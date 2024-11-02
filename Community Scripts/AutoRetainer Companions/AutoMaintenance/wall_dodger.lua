--[[
"move forwards a bit" after teleporting back to fc estate
until lim fixes this problem with AR teleporting home to weird situation ;o
loop wait for is char ready 

setup automaton -> enhanced login -> run while AR is active. 
/pcraft run moveforwardabit
or whatever you called it.
--]]

function fartknocker()
	while IsPlayerAvailable() == false do
		yield("/echo waiting on player")
		yield("/wait 1")
	end
end

--don't run this on chars with blu level > 0. they might be actual play chars and it would be annoying to move randomly on login
yield("/echo BLU -> "..tonumber(GetLevel(25)))

isblu = tonumber(GetLevel(25))

--we can add cosmic,s9,buttcheeks,etc as they get added to the game
housing_zones = 
{
136, 	--mist
340, 	--lavender beds
341, 	--goblet
641, 	--shirogane
979 	--empyreum
}

zoyn = GetZoneID()
badzoyn = 1

function bz()
	badzoyn = 1
	for i=1,#housing_zones do
		if housing_zones[i] == zoyn then
			badzoyn = 0   --dont tp if we in a housing zone.
		end
	end
end

if isblu == 0 then
	if badzoyn == 0 then --we are already in a housing zone. let's enter the nearby entrance
		yield("/echo Jumping towards entrance a bit")
	
--		yield("/ays m d") --turn ar off for now
--		yield("/ays reset") --reset ar shenanigans for now
		yield("/target entrance")
		yield("/wait 0.5")
	end		
	bz()
	if badzoyn == 0 then --we are already in a housing zone. let's enter the nearby entrance

		yield("/hold Q")

		yield("/target entrance")
		yield("/interact")
		yield("/lockon on")
		yield("/automove on")
		yield("/release Q")
		yield("/hold E")
		yield("/wait 0.5")
	end		
	bz()
	if badzoyn == 0 then --we are already in a housing zone. let's enter the nearby entrance
		yield("/gaction jump")

		yield("/release E")
		yield("/hold W <wait.0.5>")
		yield("/gaction jump")
		yield("/release W")
		yield("/interact")
		yield("/hold Q")
		yield("/wait 0.5")
	end		
	bz()
	if badzoyn == 0 then --we are already in a housing zone. let's enter the nearby entrance

		yield("/release Q")
		yield("/target entrance")
		yield("/interact")
		yield("/lockon on")
		yield("/automove on")
		yield("/wait 0.5")
	end		
	bz()
	if badzoyn == 0 then --we are already in a housing zone. let's enter the nearby entrance
		yield("/gaction jump")
--		yield("/ays m e") --turn ar back on
	end


	if badzoyn == 1 then
		yield("/waitaddon NamePlate <maxwait.600><wait.5>")
		yield("/ays m d") --turn ar off for now
		yield("/ays reset") --reset ar shenanigans for now
		yield("/wait 1")
		yield("/waitaddon NamePlate <maxwait.600><wait.5>")

		--teleport home
		yield("/li fc")
		yield("/wait 15")
		fartknocker()
		yield("/waitaddon NamePlate <maxwait.600><wait.5>")
		yield("/wait 1")
		yield("/hold W <wait.0.5>")
		yield("/release W")

		yield("/ays m e") --turn ar back on
	end
end

