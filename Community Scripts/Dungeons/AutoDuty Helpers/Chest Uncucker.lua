--for running passively while AD is doing its thing
--pandora -> loot chests on

cucked_by_chests = "often"

while cucked_by_chests == "often" do
	--safe check ifs
	if IsPlayerAvailable() then
	if type(GetCharacterCondition(34)) == "boolean" and type(GetCharacterCondition(26)) == "boolean" and type(GetCharacterCondition(4)) == "boolean" then
		--if GetCharacterCondition(26) == false and GetCharacterCondition(34) == true then --swap with next line if you just want looting while not in combat
		if GetCharacterCondition(34) == true then
			zist = GetDistanceToObject("Treasure Coffer")
			if zist < 20 and zist > 0.1 then
				yield("/vnavmesh moveto "..GetObjectRawXPos("Treasure Coffer").." "..GetObjectRawYPos("Treasure Coffer").." "..GetObjectRawZPos("Treasure Coffer"))
				yield("/echo attempting to uncuck a chest....")
			end
		end
	--safe check ends
	end
	end
	--
	yield("/wait 0.5") --script loop so we dont infinite cpu burn
end


--lapis cucknalis 1097
