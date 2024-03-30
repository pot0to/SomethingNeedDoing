--script to kind of autofollow specific person in party when not in a duty by riding their vehicule
--meant to use when your ahh botting treasure maps :~D

--requirements :
--croizats SND - disable SND targeting in config
--simpletweaks with targeting fix enabled

fren = "Frend Name" --can be partial as long as its unique
weirdvar = 1
--mker = "cross" --in case you want the other shapes. valid shapes are triangle square circle attack1-8 bind1-3 ignore1-2

--init
yield("/target "..fren)
yield("/wait 0.5")
--yield("/mk cross <t>")
--yield("/item Gysahl Greens")
local partycardinality = 2

for i=2,8 do
	if GetPartyMemberName(i) == fren then
		partycardinality = i
	end
end

yield("Friend is party slot -> "..partycardinality)

while weirdvar == 1 do
	if type(GetCharacterCondition(34)) == "boolean" and type(GetCharacterCondition(26)) == "boolean" and type(GetCharacterCondition(4)) == "boolean" then
		if GetCharacterCondition(34) == false then  --not in duty 
			--check if chocobro is up or not!
			if GetCharacterCondition(26) == false then --not in combat
				if GetCharacterCondition(4) == false then --not mounted
					--yield("/target <cross>")
					yield("/target "..fren)
					yield("/wait 0.5")
					PathfindAndMoveTo(GetObjectRawXPos(fren), GetObjectRawYPos(fren),GetObjectRawZPos(fren), false)
					--yield("/lockon on")
					--yield("/automove on")
					
					--[[yield("/ridepillion <"..mker.."> 1")
					yield("/ridepillion <"..mker.."> 2")
					yield("/ridepillion <"..mker.."> 3")]]
					yield("/ridepillion <2> 1")
					yield("/ridepillion <2> 2")
					yield("/ridepillion <2> 3")				
				end
			end
		end
	end
	yield("/wait 1")
end