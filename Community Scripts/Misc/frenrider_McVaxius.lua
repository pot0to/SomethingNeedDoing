--script to kind of autofollow specific person in party when not in a duty by riding their vehicule
--meant to use when your ahh botting treasure maps :~D

--requirements :
--croizats SND - disable SND targeting in config
--simpletweaks with targeting fix enabled

fren = "Frend Name" --can be partial as long as its unique
weirdvar = 1
mker = "cross" --in case you want the other shapes. valid shapes are triangle square circle attack1-8 bind1-3 ignore1-2

--init
yield("/target "..fren)
yield("/wait 0.5")
yield("/mk cross <t>")
yield("/item Gysahl Greens")

while weirdvar == 1 do
	if GetCharacterCondition(34) == false then  --not in duty 
		--check if chocobro is up or not!
		if GetCharacterCondition(26) == false then --not in combat
			if GetCharacterCondition(4) == false then --not mounted
				yield("/target <cross>")
				yield("/wait 0.5")
				yield("/lockon on")
				yield("/automove on")
				yield("/ridepillion <"..mker.."> 1")
				yield("/ridepillion <"..mker.."> 2")
				yield("/ridepillion <"..mker.."> 3")
			end
		end
	end
	yield("/wait 1")
end