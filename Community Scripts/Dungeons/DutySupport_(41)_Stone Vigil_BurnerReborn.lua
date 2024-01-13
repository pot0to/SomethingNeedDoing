--Fully working Script for Auto Dungeons- Example is the Stone Vigil

local waypoints = {
{x = 0, y = 4, z = 305, movement = "rebuild"},
{x = 0, y = 4, z = 305, movement = "navmesh"}, -- Example coordinates
{x = 1, y = 8, z = 268, movement = "navmesh"},
{x = 31, y = 8, z = 218, movement = "navmesh"},
{x = 9, y = 8, z = 206, movement = "navmesh"},
{x = -30, y = 8, z = 232, movement = "navmesh"},
{x = -31, y = 8, z = 182, movement = "navmesh"},
{x = 2, y = 8, z = 167, movement = "navmesh"},
{x = 0, y = 4, z = 147, movement = "navmesh"},
{x = 0, y = 0, z = 107, movement = "visland"},
{x = 0, y = 0, z = 107, movement = "navmesh"},
{x = -1, y = 0, z = 48, movement = "visland"},
{x = -1, y = 0, z = 48, movement = "rebuild"},
{x = -48, y = -4, z = 47, movement = "navmesh"},
{x = -48, y = -4, z = 26, movement = "navmesh"},
{x = -6, y = 1, z = 24, movement = "navmesh"},
{x = 24, y = 0, z = 14, movement = "navmesh"},
{x = 22, y = 0, z = -18, movement = "navmesh"},
{x = 8, y = 1, z = -58, movement = "navmesh"},
{x = 20, y = 4, z = -80, movement = "navmesh"},
{x = 54, y = 4, z = -80, movement = "visland"},
{x = 35, y = 4, z = -80, movement = "navmesh"},
{x = 0, y = 0, z = -80, movement = "visland"},
{x = 0, y = 0, z = -100, movement = "visland"},
{x = 21, y = 0, z = -104, movement = "visland"},
{x = 21, y = 0, z = -104, movement = "rebuild"},
{x = 24, y = 0, z = -119, movement = "navmesh"},
{x = 24, y = 0, z = -162, movement = "navmesh"},
{x = 4, y = 0, z = -183, movement = "navmesh"},
{x = 0, y = 4, z = -212, movement = "navmesh"},
{x = 0, y = 4, z = -212, movement = "interact"},
{x = -1, y = 0, z = -251, movement = "visland"},
{x = 0, y = 0, z = -266, movement = "navmesh"},
{x = 0, y = 0, z = -266, movement = "interact"},
}

local currentWaypointIndex = 1

while currentWaypointIndex <= #waypoints do
    local waypoint = waypoints[currentWaypointIndex]
    
        if not GetCharacterCondition(26) then
        if waypoint.movement == "navmesh" then
        yield("/vnavmesh moveto " .. waypoint.x .. " " .. waypoint.y .. " " .. waypoint.z)
    elseif waypoint.movement == "visland" then
        yield("/visland moveto " .. waypoint.x .. " " .. waypoint.y .. " " .. waypoint.z)
    elseif waypoint.movement == "interact" then
            yield("/send NUMPAD0")
            yield("/wait 1")
            yield("/send NUMPAD0")
            yield("/wait 3")
    elseif waypoint.movement == "rebuild" then
            yield("/vnavmesh rebuild")
            yield("/wait 10")
    end

        local playerX = GetPlayerRawXPos()
        local playerY = GetPlayerRawYPos()
        local playerZ = GetPlayerRawZPos()

        local distance = GetDistanceToPoint(waypoint.x, waypoint.y, waypoint.z)

        if distance <= 5.0 then -- Assuming 1.0 as a threshold for reaching the waypoint
            currentWaypointIndex = currentWaypointIndex + 1
        end
end

        yield("/wait 2")  -- Short delay to check the position
end
