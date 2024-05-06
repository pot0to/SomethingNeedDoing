--script to kind of autofollow specific person in party when not in a duty by riding their vehicule
--meant to use when your ahh botting treasure maps :~D

--[[
*requirements:
croizats SND - disable SND targeting in config
simpletweaks with targeting fix enabled
vnavmesh
visland

*optional:
bring some gysahl greens
]]

--1 small problem
---> sometimes if a char is stranded they will run to who knows where, do we need to put a max dist too say 30 yalms

---------CONFIGURATION SECTION---------
fren = "Frend Name" 	--can be partial as long as its unique
cling = 0.5 			--distance to cling to fren
clingy = true			--are we clingy? if not then the fren will have to swoop by to pick them up. recommend ON unless your doing quests or something.
--mker = "cross" --in case you want the other shapes. valid shapes are triangle square circle attack1-8 bind1-3 ignore1-2
-----------CONFIGURATION END-----------

--init
yield("/echo Starting fren rider")
--yield("/target \""..fren.."\"")
yield("/wait 0.5")
--yield("/mk cross <t>")

local function distance(x1, y1, z1, x2, y2, z2)
	if type(x1) ~= "number" then x1 = 0 end
	if type(y1) ~= "number" then y1 = 0 end
	if type(z1) ~= "number" then y1 = 0 end
	if type(x2) ~= "number" then x2 = 0 end
	if type(y2) ~= "number" then y2	= 0 end
	if type(z2) ~= "number" then z2 = 0 end
	zoobz = math.sqrt((x2 - x1)^2 + (y2 - y1)^2 + (z2 - z1)^2)
	if type(zoobz) ~= "number" then
		zoobz = 0
	end
    --return math.sqrt((x2 - x1)^2 + (y2 - y1)^2 + (z2 - z1)^2)
    return zoobz
end

weirdvar = 1
local partycardinality = 2
for i=0,7 do
	if GetPartyMemberName(i) == fren then
		partycardinality = i
	end
end
--turns out the above is worthless and not what i wanted.
local fartycardinality = 2
local countfartula = 2
while countfartula < 9 do
	yield("/target <"..countfartula..">")
	yield("/wait 0.5")
	yield("/echo is it "..GetTargetName().."?")
	if GetTargetName() == fren then
		fartycardinality = countfartula
		countfartula = 9
	end
	countfartula = countfartula + 1
end

--yield("Friend is party slot -> "..partycardinality.." but actually is ff14 slot -> "..fartycardinality)
yield("Friend is party slot -> "..fartycardinality)
ClearTarget()


while weirdvar == 1 do
	--catch if character is ready before doing anything
	if IsPlayerAvailable() then
		if type(GetCharacterCondition(34)) == "boolean" and type(GetCharacterCondition(26)) == "boolean" and type(GetCharacterCondition(4)) == "boolean" then
			if GetCharacterCondition(34) == false then  --not in duty 
				--check if chocobro is up or not! we can't do it yet
				if GetCharacterCondition(26) == true then --in combat
						if clingy then
							--check distance to fren, if its more than cling, then
							if distance(GetPlayerRawXPos(), GetPlayerRawYPos(), GetPlayerRawZPos(), GetObjectRawXPos(fren),GetObjectRawYPos(fren),GetObjectRawZPos(fren)) >  cling then
							--yield("/target \""..fren.."\"")
								PathfindAndMoveTo(GetObjectRawXPos(fren),GetObjectRawYPos(fren),GetObjectRawZPos(fren), false)
							end
							yield("/wait 0.5")
						end	
				end
				if GetCharacterCondition(26) == false then --not in combat
					if GetCharacterCondition(4) == false and GetCharacterCondition(10) == false then --not mounted and notmounted2 (riding friend)
						if GetBuddyTimeRemaining() < 900 and GetItemCount(4868) > 0 then
							yield("/visland stop")
							yield("/vnavmesh stop")
							yield("/item Gysahl Greens")
							yield("/wait 2")
						end
						--yield("/target <cross>")
						if clingy then
							--check distance to fren, if its more than cling, then
							if distance(GetPlayerRawXPos(), GetPlayerRawYPos(), GetPlayerRawZPos(), GetObjectRawXPos(fren),GetObjectRawYPos(fren),GetObjectRawZPos(fren)) >  cling then
							--yield("/target \""..fren.."\"")
								PathfindAndMoveTo(GetObjectRawXPos(fren),GetObjectRawYPos(fren),GetObjectRawZPos(fren), false)
							end
							yield("/wait 0.5")
						end	
						--yield("/lockon on")
						--yield("/automove on")

						--[[yield("/ridepillion <"..mker.."> 1")
						yield("/ridepillion <"..mker.."> 2")
						yield("/ridepillion <"..mker.."> 3")]]
						if IsPartyMemberMounted(partycardinality) == true then
							--for i=1,7 do
								--yield("/ridepillion <"..partycardinality.."> "..i)
								yield("/ridepillion <"..fartycardinality.."> 2")
							--end
							yield("/echo Attempting to Mount Friend")
							yield("/wait 0.5")
						end
					end
				end
			end
		end
	end
	yield("/wait 1")
end