--jiggle it a bit and make it work for autoduty
--purpose. AD gets stuck randomly for me.  This solves that. 
--instructions:
--none. it works on my machine (tm)

wheahehhahehhaheohuahoeuhaosdflkj = 1

while wheahehhahehhaheohuahoeuhaosdflkj == 1 do
	x1 = GetPlayerRawXPos()
	y1 = GetPlayerRawYPos()
	z1 = GetPlayerRawZPos()
	yield("/wait 20")
	if (x1 - GetPlayerRawXPos()) == 0 and (y1 - GetPlayerRawYPos()) == 0 and (z1 - GetPlayerRawZPos()) == 0 then
		yield("/echo We havent moved in 20 seconds. its time to slap the machine or jiggle it a bit")
		yield("/pcall SelectYesno true 0")
		yield("/wait 1")
		yield("/pcall Repair true 0")
		yield("/wait 1")
		yield("/pcall Repair true 1")
		yield("/wait 3")
		yield("/pcall SelectYesno true 0")
		yield("/wait 1")
		while not NavIsReady() do
			yield("/echo waiting on navmesh to finish rebuilding the mesh")
			yield("/wait 1")
		end
		--[[
		if GetCharacterCondition(34) == false then
			yield("/ad stop")
			yield("/wait 2")
			yield("/ad start")
			yield("/wait 2")
		end
		--]]
	end
end