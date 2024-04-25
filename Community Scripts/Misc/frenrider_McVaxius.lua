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

fren = "Frend Name" --can be partial as long as its unique
weirdvar = 1
--mker = "cross" --in case you want the other shapes. valid shapes are triangle square circle attack1-8 bind1-3 ignore1-2

--init
yield("/target \""..fren.."\"")
yield("/wait 0.5")
--yield("/mk cross <t>")
local partycardinality = 2

for i=0,7 do
	if GetPartyMemberName(i) == fren then
		partycardinality = i
	end
end

yield("Friend is party slot -> "..partycardinality)

while weirdvar == 1 do
	if type(GetCharacterCondition(34)) == "boolean" and type(GetCharacterCondition(26)) == "boolean" and type(GetCharacterCondition(4)) == "boolean" then
		if GetCharacterCondition(34) == false then  --not in duty 
			--check if chocobro is up or not! we can't do it yet
			if GetCharacterCondition(26) == false then --not in combat
				if GetCharacterCondition(4) == false then --not mounted
					if GetBuddyTimeRemaining() < 900 and GetItemCount(4868) > 0 then
						yield("/visland stop")
						yield("/vnavmesh stop")
						yield("/item Gysahl Greens")
						yield("/wait 2")
					end
					--yield("/target <cross>")
					yield("/target \""..fren.."\"")
						yield("/wait 0.5")
						PathfindAndMoveTo(GetObjectRawXPos(fren),GetObjectRawYPos(fren),GetObjectRawZPos(fren), false)
						--yield("/lockon on")
					--yield("/automove on")
					
					--[[yield("/ridepillion <"..mker.."> 1")
					yield("/ridepillion <"..mker.."> 2")
					yield("/ridepillion <"..mker.."> 3")]]
					if IsPartyMemberMounted(partycardinality) == true then
						for i=1,7 do
							yield("/ridepillion <"..partycardinality.."> "..i)
						end
						yield("/echo Attempting to Mount Friend")
						yield("/wait 0.5")
					end
				end
			end
		end
	end
	yield("/wait 1")
end