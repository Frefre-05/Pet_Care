--[[
    Last to Leave the Circle Wins - Roblox Studio Command Bar Installer

    How to use:
    1. Open Roblox Studio.
    2. Open your place.
    3. Open View > Command Bar.
    4. Paste this entire script into the Command Bar and press Enter.

    What this creates:
    - ServerScriptService/LastToLeaveCircleServer
    - ReplicatedStorage/LastToLeaveCircleUpdate
    - StarterGui/LastToLeaveCircleGui
    - Workspace/LastToLeaveCircleMap with a safe-zone cylinder, spawn platform, death area, hazards folder, and sound part

    The generated game:
    - Waits for enough players, or starts with 1 player plus 2 NPC survivors.
    - Starts everyone inside a circular safe zone.
    - Shrinks the circle smoothly.
    - Instantly eliminates players and NPCs outside the circle.
    - Ends when one player remains alive.
    - Resets for the next round.
]]

local ServerScriptService = game:GetService("ServerScriptService")
local ReplicatedStorage = game:GetService("ReplicatedStorage")
local StarterGui = game:GetService("StarterGui")
local Workspace = game:GetService("Workspace")

local function deleteIfExists(parent, name)
    local existing = parent:FindFirstChild(name)
    if existing then
        existing:Destroy()
    end
end

deleteIfExists(ServerScriptService, "LastToLeaveCircleServer")
deleteIfExists(ReplicatedStorage, "LastToLeaveCircleUpdate")
deleteIfExists(ReplicatedStorage, "LastToLeaveCircleShop")
deleteIfExists(StarterGui, "LastToLeaveCircleGui")
deleteIfExists(Workspace, "LastToLeaveCircleMap")

local updateEvent = Instance.new("RemoteEvent")
updateEvent.Name = "LastToLeaveCircleUpdate"
updateEvent.Parent = ReplicatedStorage

local shopEvent = Instance.new("RemoteEvent")
shopEvent.Name = "LastToLeaveCircleShop"
shopEvent.Parent = ReplicatedStorage

local mapFolder = Instance.new("Folder")
mapFolder.Name = "LastToLeaveCircleMap"
mapFolder.Parent = Workspace

local platform = Instance.new("Part")
platform.Name = "RoundPlatform"
platform.Anchored = true
platform.Size = Vector3.new(170, 2, 170)
platform.Position = Vector3.new(0, 0, 0)
platform.Material = Enum.Material.Concrete
platform.Color = Color3.fromRGB(70, 78, 88)
platform.TopSurface = Enum.SurfaceType.Smooth
platform.BottomSurface = Enum.SurfaceType.Smooth
platform.Parent = mapFolder

local safeZone = Instance.new("Part")
safeZone.Name = "SafeZone"
safeZone.Anchored = true
safeZone.CanCollide = false
safeZone.CanTouch = false
safeZone.CanQuery = false
safeZone.Shape = Enum.PartType.Cylinder
safeZone.Material = Enum.Material.Neon
safeZone.Color = Color3.fromRGB(0, 220, 150)
safeZone.Transparency = 0.55
safeZone.Size = Vector3.new(0.2, 130, 130)
safeZone.CFrame = CFrame.new(0, 1.15, 0) * CFrame.Angles(0, 0, math.rad(90))
safeZone.Parent = mapFolder

local soundPart = Instance.new("Part")
soundPart.Name = "SoundPart"
soundPart.Anchored = true
soundPart.CanCollide = false
soundPart.Transparency = 1
soundPart.Size = Vector3.new(2, 2, 2)
soundPart.Position = Vector3.new(0, 8, 0)
soundPart.Parent = mapFolder

local shrinkSound = Instance.new("Sound")
shrinkSound.Name = "CircleShrinkSound"
shrinkSound.SoundId = "rbxassetid://9118823109"
shrinkSound.Volume = 0.35
shrinkSound.RollOffMaxDistance = 180
shrinkSound.Parent = soundPart

local deathPlatform = Instance.new("Part")
deathPlatform.Name = "DeathPlatform"
deathPlatform.Anchored = true
deathPlatform.Size = Vector3.new(70, 2, 70)
deathPlatform.Position = Vector3.new(0, 0, 230)
deathPlatform.Material = Enum.Material.Concrete
deathPlatform.Color = Color3.fromRGB(120, 35, 45)
deathPlatform.TopSurface = Enum.SurfaceType.Smooth
deathPlatform.BottomSurface = Enum.SurfaceType.Smooth
deathPlatform.Parent = mapFolder

local deathSpawn = Instance.new("SpawnLocation")
deathSpawn.Name = "DeathAreaSpawn"
deathSpawn.Anchored = true
deathSpawn.Neutral = true
deathSpawn.Enabled = false
deathSpawn.Size = Vector3.new(8, 1, 8)
deathSpawn.Position = Vector3.new(0, 4, 230)
deathSpawn.Material = Enum.Material.Neon
deathSpawn.Color = Color3.fromRGB(255, 70, 80)
deathSpawn.Transparency = 0.25
deathSpawn.Parent = mapFolder

local deathLabel = Instance.new("Part")
deathLabel.Name = "DeathAreaLabel"
deathLabel.Anchored = true
deathLabel.CanCollide = false
deathLabel.Size = Vector3.new(18, 6, 1)
deathLabel.Position = Vector3.new(0, 9, 195)
deathLabel.Material = Enum.Material.SmoothPlastic
deathLabel.Color = Color3.fromRGB(35, 35, 35)
deathLabel.Parent = mapFolder

local deathGui = Instance.new("SurfaceGui")
deathGui.Face = Enum.NormalId.Front
deathGui.SizingMode = Enum.SurfaceGuiSizingMode.PixelsPerStud
deathGui.PixelsPerStud = 45
deathGui.Parent = deathLabel

local deathText = Instance.new("TextLabel")
deathText.BackgroundTransparency = 1
deathText.Size = UDim2.new(1, 0, 1, 0)
deathText.Font = Enum.Font.GothamBold
deathText.Text = "ELIMINATED AREA"
deathText.TextColor3 = Color3.fromRGB(255, 255, 255)
deathText.TextScaled = true
deathText.Parent = deathGui

local hazardsFolder = Instance.new("Folder")
hazardsFolder.Name = "Hazards"
hazardsFolder.Parent = mapFolder

local gui = Instance.new("ScreenGui")
gui.Name = "LastToLeaveCircleGui"
gui.ResetOnSpawn = false
gui.IgnoreGuiInset = false
gui.Parent = StarterGui

local frame = Instance.new("Frame")
frame.Name = "StatusFrame"
frame.AnchorPoint = Vector2.new(0.5, 0)
frame.Position = UDim2.new(0.5, 0, 0, 16)
frame.Size = UDim2.new(0, 360, 0, 86)
frame.BackgroundColor3 = Color3.fromRGB(20, 24, 28)
frame.BackgroundTransparency = 0.15
frame.BorderSizePixel = 0
frame.Parent = gui

local corner = Instance.new("UICorner")
corner.CornerRadius = UDim.new(0, 8)
corner.Parent = frame

local statusLabel = Instance.new("TextLabel")
statusLabel.Name = "StatusLabel"
statusLabel.BackgroundTransparency = 1
statusLabel.Position = UDim2.new(0, 12, 0, 8)
statusLabel.Size = UDim2.new(1, -24, 0, 28)
statusLabel.Font = Enum.Font.GothamBold
statusLabel.Text = "Waiting for players..."
statusLabel.TextColor3 = Color3.fromRGB(255, 255, 255)
statusLabel.TextScaled = true
statusLabel.Parent = frame

local timeLabel = Instance.new("TextLabel")
timeLabel.Name = "TimeLabel"
timeLabel.BackgroundTransparency = 1
timeLabel.Position = UDim2.new(0, 12, 0, 42)
timeLabel.Size = UDim2.new(0.5, -18, 0, 28)
timeLabel.Font = Enum.Font.Gotham
timeLabel.Text = "Time: --"
timeLabel.TextColor3 = Color3.fromRGB(220, 235, 245)
timeLabel.TextScaled = true
timeLabel.Parent = frame

local aliveLabel = Instance.new("TextLabel")
aliveLabel.Name = "AliveLabel"
aliveLabel.BackgroundTransparency = 1
aliveLabel.Position = UDim2.new(0.5, 6, 0, 42)
aliveLabel.Size = UDim2.new(0.5, -18, 0, 28)
aliveLabel.Font = Enum.Font.Gotham
aliveLabel.Text = "Alive: 0"
aliveLabel.TextColor3 = Color3.fromRGB(220, 235, 245)
aliveLabel.TextScaled = true
aliveLabel.Parent = frame

local shopButton = Instance.new("TextButton")
shopButton.Name = "ShopButton"
shopButton.AnchorPoint = Vector2.new(1, 0)
shopButton.Position = UDim2.new(1, -16, 0, 112)
shopButton.Size = UDim2.new(0, 150, 0, 42)
shopButton.BackgroundColor3 = Color3.fromRGB(35, 125, 95)
shopButton.BorderSizePixel = 0
shopButton.Font = Enum.Font.GothamBold
shopButton.Text = "Shop"
shopButton.TextColor3 = Color3.fromRGB(255, 255, 255)
shopButton.TextScaled = true
shopButton.Parent = gui

local shopButtonCorner = Instance.new("UICorner")
shopButtonCorner.CornerRadius = UDim.new(0, 8)
shopButtonCorner.Parent = shopButton

local shopFrame = Instance.new("Frame")
shopFrame.Name = "ShopFrame"
shopFrame.AnchorPoint = Vector2.new(1, 0)
shopFrame.Position = UDim2.new(1, -16, 0, 164)
shopFrame.Size = UDim2.new(0, 300, 0, 250)
shopFrame.BackgroundColor3 = Color3.fromRGB(22, 27, 31)
shopFrame.BackgroundTransparency = 0.08
shopFrame.BorderSizePixel = 0
shopFrame.Visible = false
shopFrame.Parent = gui

local shopCorner = Instance.new("UICorner")
shopCorner.CornerRadius = UDim.new(0, 8)
shopCorner.Parent = shopFrame

local coinsLabel = Instance.new("TextLabel")
coinsLabel.Name = "CoinsLabel"
coinsLabel.BackgroundTransparency = 1
coinsLabel.Position = UDim2.new(0, 12, 0, 10)
coinsLabel.Size = UDim2.new(1, -24, 0, 32)
coinsLabel.Font = Enum.Font.GothamBold
coinsLabel.Text = "Coins: 0"
coinsLabel.TextColor3 = Color3.fromRGB(255, 235, 120)
coinsLabel.TextScaled = true
coinsLabel.Parent = shopFrame

local function makeShopItem(name, yPosition, text)
    local button = Instance.new("TextButton")
    button.Name = name
    button.Position = UDim2.new(0, 12, 0, yPosition)
    button.Size = UDim2.new(1, -24, 0, 48)
    button.BackgroundColor3 = Color3.fromRGB(45, 55, 64)
    button.BorderSizePixel = 0
    button.Font = Enum.Font.Gotham
    button.Text = text
    button.TextColor3 = Color3.fromRGB(255, 255, 255)
    button.TextScaled = true
    button.Parent = shopFrame

    local buttonCorner = Instance.new("UICorner")
    buttonCorner.CornerRadius = UDim.new(0, 8)
    buttonCorner.Parent = button

    return button
end

makeShopItem("SpeedUpgrade", 56, "Speed +2 - 20 coins")
makeShopItem("JumpUpgrade", 112, "Jump +5 - 20 coins")
makeShopItem("ShieldUpgrade", 168, "One Shield - 35 coins")

local guiScript = Instance.new("LocalScript")
guiScript.Name = "GuiUpdater"
guiScript.Source = [[
local Players = game:GetService("Players")
local ReplicatedStorage = game:GetService("ReplicatedStorage")

local player = Players.LocalPlayer
local updateEvent = ReplicatedStorage:WaitForChild("LastToLeaveCircleUpdate")
local shopEvent = ReplicatedStorage:WaitForChild("LastToLeaveCircleShop")
local gui = script.Parent
local frame = gui:WaitForChild("StatusFrame")
local statusLabel = frame:WaitForChild("StatusLabel")
local timeLabel = frame:WaitForChild("TimeLabel")
local aliveLabel = frame:WaitForChild("AliveLabel")
local shopButton = gui:WaitForChild("ShopButton")
local shopFrame = gui:WaitForChild("ShopFrame")
local coinsLabel = shopFrame:WaitForChild("CoinsLabel")
local speedUpgrade = shopFrame:WaitForChild("SpeedUpgrade")
local jumpUpgrade = shopFrame:WaitForChild("JumpUpgrade")
local shieldUpgrade = shopFrame:WaitForChild("ShieldUpgrade")

local function updateCoins()
    local leaderstats = player:FindFirstChild("leaderstats")
    local coins = leaderstats and leaderstats:FindFirstChild("Coins")
    coinsLabel.Text = "Coins: " .. tostring(coins and coins.Value or 0)
end

updateEvent.OnClientEvent:Connect(function(statusText, timeRemaining, playersAlive)
    statusLabel.Text = tostring(statusText or "")

    if typeof(timeRemaining) == "number" and timeRemaining >= 0 then
        timeLabel.Text = "Time: " .. tostring(math.ceil(timeRemaining))
    else
        timeLabel.Text = "Time: --"
    end

    aliveLabel.Text = "Alive: " .. tostring(playersAlive or 0)
end)

shopButton.Activated:Connect(function()
    shopFrame.Visible = not shopFrame.Visible
end)

speedUpgrade.Activated:Connect(function()
    shopEvent:FireServer("Speed")
end)

jumpUpgrade.Activated:Connect(function()
    shopEvent:FireServer("Jump")
end)

shieldUpgrade.Activated:Connect(function()
    shopEvent:FireServer("Shield")
end)

local leaderstats = player:WaitForChild("leaderstats")
local coins = leaderstats:WaitForChild("Coins")
coins:GetPropertyChangedSignal("Value"):Connect(updateCoins)
updateCoins()
]]
guiScript.Parent = gui

local serverScript = Instance.new("Script")
serverScript.Name = "LastToLeaveCircleServer"
serverScript.Source = [[
local Players = game:GetService("Players")
local ReplicatedStorage = game:GetService("ReplicatedStorage")
local RunService = game:GetService("RunService")
local TweenService = game:GetService("TweenService")
local Workspace = game:GetService("Workspace")

local updateEvent = ReplicatedStorage:WaitForChild("LastToLeaveCircleUpdate")
local shopEvent = ReplicatedStorage:WaitForChild("LastToLeaveCircleShop")

local mapFolder = Workspace:WaitForChild("LastToLeaveCircleMap")
local platform = mapFolder:WaitForChild("RoundPlatform")
local safeZone = mapFolder:WaitForChild("SafeZone")
local soundPart = mapFolder:WaitForChild("SoundPart")
local deathSpawn = mapFolder:WaitForChild("DeathAreaSpawn")
local hazardsFolder = mapFolder:WaitForChild("Hazards")
local shrinkSound = soundPart:WaitForChild("CircleShrinkSound")

local MIN_PLAYERS = 1
local INTERMISSION_TIME = 12
local ROUND_TIME = 75
local START_RADIUS = 65
local END_RADIUS = 5
local SAFE_ZONE_HEIGHT = 0.2
local WIN_COINS = 50
local CENTER = Vector3.new(0, 1.15, 0)
local SPAWN_HEIGHT = 8
local PLAYER_SPAWN_HEIGHT = 12
local NPC_MOVE_HEIGHT = 3
local CONTESTANT_RADIUS = 2.5
local LASER_CENTER_HEIGHT = 5
local LASER_HIT_HEIGHT = 8
local DEATH_SPAWN_POSITION = Vector3.new(0, 8, 230)

local roundActive = false
local alivePlayers = {}
local aliveNpcs = {}
local roundNpcs = {}
local playerPerks = {}
local activeConnections = {}
local currentGravity = Workspace.Gravity

local SHOP_ITEMS = {
    Speed = {
        Cost = 20,
        Max = 4
    },
    Jump = {
        Cost = 20,
        Max = 4
    },
    Shield = {
        Cost = 35,
        Max = 1
    }
}

local function setupPlayerData(player)
    if not playerPerks[player] then
        playerPerks[player] = {
            Speed = 0,
            Jump = 0,
            Shield = 0
        }
    end

    local leaderstats = player:FindFirstChild("leaderstats")
    if not leaderstats then
        leaderstats = Instance.new("Folder")
        leaderstats.Name = "leaderstats"
        leaderstats.Parent = player
    end

    local coins = leaderstats:FindFirstChild("Coins")
    if not coins then
        coins = Instance.new("IntValue")
        coins.Name = "Coins"
        coins.Value = 0
        coins.Parent = leaderstats
    end

    local wins = leaderstats:FindFirstChild("Wins")
    if not wins then
        wins = Instance.new("IntValue")
        wins.Name = "Wins"
        wins.Value = 0
        wins.Parent = leaderstats
    end
end

local function getCoinsValue(player)
    setupPlayerData(player)
    return player.leaderstats.Coins
end

local function getWinsValue(player)
    setupPlayerData(player)
    return player.leaderstats.Wins
end

local function applyPlayerPerks(player)
    local perks = playerPerks[player]
    local character = player.Character
    local humanoid = character and character:FindFirstChildOfClass("Humanoid")

    if not perks or not humanoid then
        return
    end

    humanoid.WalkSpeed = 16 + (perks.Speed * 2)
    humanoid.JumpPower = 50 + (perks.Jump * 5)
end

local function unstickCharacter(character)
    local root = character and character:FindFirstChild("HumanoidRootPart")
    local humanoid = character and character:FindFirstChildOfClass("Humanoid")

    if root then
        root.Anchored = false
        root.AssemblyLinearVelocity = Vector3.zero
        root.AssemblyAngularVelocity = Vector3.zero
    end

    if humanoid then
        humanoid.Sit = false
        humanoid.PlatformStand = false
        humanoid:ChangeState(Enum.HumanoidStateType.GettingUp)
    end
end

local function broadcast(statusText, timeRemaining)
    local aliveCount = 0
    for _, isAlive in pairs(alivePlayers) do
        if isAlive then
            aliveCount += 1
        end
    end

    for _, isAlive in pairs(aliveNpcs) do
        if isAlive then
            aliveCount += 1
        end
    end

    updateEvent:FireAllClients(statusText, timeRemaining, aliveCount)
end

local function getAliveContestants()
    local list = {}

    for player, isAlive in pairs(alivePlayers) do
        if isAlive then
            local character = player.Character
            local humanoid = character and character:FindFirstChildOfClass("Humanoid")

            if humanoid and humanoid.Health > 0 then
                table.insert(list, {
                    Name = player.Name,
                    Character = character,
                    Type = "Player",
                    Player = player
                })
            else
                alivePlayers[player] = false
            end
        end
    end

    for npc, isAlive in pairs(aliveNpcs) do
        if isAlive then
            local humanoid = npc:FindFirstChildOfClass("Humanoid")

            if humanoid and humanoid.Health > 0 then
                table.insert(list, {
                    Name = npc.Name,
                    Character = npc,
                    Type = "NPC",
                    Model = npc
                })
            else
                aliveNpcs[npc] = false
            end
        end
    end

    return list
end

local function getAlivePlayers()
    local list = {}

    for player, isAlive in pairs(alivePlayers) do
        if isAlive then
            local character = player.Character
            local humanoid = character and character:FindFirstChildOfClass("Humanoid")

            if humanoid and humanoid.Health > 0 then
                table.insert(list, player)
            else
                alivePlayers[player] = false
            end
        end
    end

    return list
end

local function setSafeZoneRadius(radius)
    -- Roblox cylinder parts use X as the cylinder length. Rotate X upward so the
    -- circular face lies flat on the arena floor.
    safeZone.Size = Vector3.new(SAFE_ZONE_HEIGHT, radius * 2, radius * 2)
    safeZone.CFrame = CFrame.new(CENTER) * CFrame.Angles(0, 0, math.rad(90))
end

local function getSafeZoneRadius()
    return safeZone.Size.Y / 2
end

local function isCharacterInsideCircle(character)
    local root = character and character:FindFirstChild("HumanoidRootPart")
    if not root then
        return false
    end

    local flatRootPosition = Vector3.new(root.Position.X, 0, root.Position.Z)
    local flatCenterPosition = Vector3.new(CENTER.X, 0, CENTER.Z)
    local distanceFromCenter = (flatRootPosition - flatCenterPosition).Magnitude

    return distanceFromCenter <= math.max(getSafeZoneRadius() - CONTESTANT_RADIUS, 0)
end

local function eliminateIfOutside(character)
    local humanoid = character and character:FindFirstChildOfClass("Humanoid")

    if humanoid and humanoid.Health > 0 and not isCharacterInsideCircle(character) then
        local player = Players:GetPlayerFromCharacter(character)
        local perks = player and playerPerks[player]

        if perks and perks.Shield and perks.Shield > 0 then
            perks.Shield -= 1
            local root = character:FindFirstChild("HumanoidRootPart")
            if root then
                root.CFrame = CFrame.new(CENTER.X, PLAYER_SPAWN_HEIGHT, CENTER.Z)
                unstickCharacter(character)
            end
            return
        end

        humanoid.Health = 0
    end
end

local function eliminateCharacter(character)
    local humanoid = character and character:FindFirstChildOfClass("Humanoid")

    if humanoid and humanoid.Health > 0 then
        local player = Players:GetPlayerFromCharacter(character)
        local perks = player and playerPerks[player]

        if perks and perks.Shield and perks.Shield > 0 then
            perks.Shield -= 1
            local root = character:FindFirstChild("HumanoidRootPart")
            if root then
                root.CFrame = CFrame.new(CENTER.X, PLAYER_SPAWN_HEIGHT, CENTER.Z)
                unstickCharacter(character)
            end
            return
        end

        humanoid.Health = 0
    end
end

local function isRootInsidePart(root, part, extraPadding)
    local localPosition = part.CFrame:PointToObjectSpace(root.Position)
    local halfSize = (part.Size / 2) + Vector3.new(extraPadding, extraPadding, extraPadding)

    return math.abs(localPosition.X) <= halfSize.X
        and math.abs(localPosition.Y) <= halfSize.Y
        and math.abs(localPosition.Z) <= halfSize.Z
end

local function eliminateIfTouchingHazard(character)
    local root = character and character:FindFirstChild("HumanoidRootPart")
    local humanoid = character and character:FindFirstChildOfClass("Humanoid")

    if not root or not humanoid or humanoid.Health <= 0 then
        return
    end

    for _, hazard in ipairs(hazardsFolder:GetChildren()) do
        if hazard:IsA("BasePart") and isRootInsidePart(root, hazard, 2.5) then
            eliminateCharacter(character)
            return
        end
    end
end

local function teleportPlayerToDeathArea(player)
    player:LoadCharacter()

    local character = player.Character or player.CharacterAdded:Wait()
    local root = character:WaitForChild("HumanoidRootPart", 8)

    if root then
        root.CFrame = CFrame.new(DEATH_SPAWN_POSITION)
        unstickCharacter(character)
    end
end

local function getRandomPointInsideCircle(edgePadding)
    local radius = math.max(getSafeZoneRadius() - edgePadding, 4)
    local angle = math.random() * math.pi * 2
    local distance = math.sqrt(math.random()) * radius

    return Vector3.new(
        CENTER.X + math.cos(angle) * distance,
        NPC_MOVE_HEIGHT,
        CENTER.Z + math.sin(angle) * distance
    )
end

local function teleportPlayerIntoCircle(player, index, totalPlayers)
    player:LoadCharacter()

    local character = player.Character or player.CharacterAdded:Wait()
    local root = character:WaitForChild("HumanoidRootPart", 8)
    if not root then
        return
    end

    local angle = (index / math.max(totalPlayers, 1)) * math.pi * 2
    local spawnRadius = math.min(START_RADIUS * 0.45, 28)
    local x = math.cos(angle) * spawnRadius
    local z = math.sin(angle) * spawnRadius

    root.CFrame = CFrame.new(CENTER.X + x, PLAYER_SPAWN_HEIGHT, CENTER.Z + z)
    unstickCharacter(character)
end

local function clearRoundConnections()
    for _, connection in ipairs(activeConnections) do
        connection:Disconnect()
    end

    table.clear(activeConnections)
end

local function clearRoundNpcs()
    for _, npc in ipairs(roundNpcs) do
        if npc and npc.Parent then
            npc:Destroy()
        end
    end

    table.clear(roundNpcs)
    table.clear(aliveNpcs)
end

local function clearHazards()
    hazardsFolder:ClearAllChildren()
end

local function resetPlayerStats(player)
    applyPlayerPerks(player)
end

local function resetRound()
    roundActive = false
    clearRoundConnections()
    clearRoundNpcs()
    clearHazards()
    table.clear(alivePlayers)
    Workspace.Gravity = currentGravity
    platform.Color = Color3.fromRGB(70, 78, 88)
    setSafeZoneRadius(START_RADIUS)
    safeZone.Color = Color3.fromRGB(0, 220, 150)
    safeZone.Transparency = 0.55

    for _, player in ipairs(Players:GetPlayers()) do
        resetPlayerStats(player)
        player:LoadCharacter()
    end
end

Players.PlayerAdded:Connect(function(player)
    setupPlayerData(player)
end)

for _, player in ipairs(Players:GetPlayers()) do
    setupPlayerData(player)
end

shopEvent.OnServerEvent:Connect(function(player, itemName)
    setupPlayerData(player)

    local item = SHOP_ITEMS[itemName]
    local perks = playerPerks[player]
    local coins = getCoinsValue(player)

    if not item or not perks then
        return
    end

    if perks[itemName] >= item.Max then
        updateEvent:FireClient(player, "You already own the max " .. itemName .. " upgrade", nil, #getAliveContestants())
        return
    end

    if coins.Value < item.Cost then
        updateEvent:FireClient(player, "Not enough coins for " .. itemName, nil, #getAliveContestants())
        return
    end

    coins.Value -= item.Cost
    perks[itemName] += 1
    applyPlayerPerks(player)
    updateEvent:FireClient(player, "Bought " .. itemName .. " upgrade", nil, #getAliveContestants())
end)

local function createSurvivorNpc(index, totalNpcs)
    local npc = Instance.new("Model")
    npc.Name = "Circle Survivor NPC " .. tostring(index)

    local root = Instance.new("Part")
    root.Name = "HumanoidRootPart"
    root.Size = Vector3.new(2, 2, 1)
    root.Anchored = true
    root.CanCollide = false
    root.Color = Color3.fromRGB(240, 205, 90)
    root.Material = Enum.Material.SmoothPlastic
    root.TopSurface = Enum.SurfaceType.Smooth
    root.BottomSurface = Enum.SurfaceType.Smooth
    root.Parent = npc

    local head = Instance.new("Part")
    head.Name = "Head"
    head.Size = Vector3.new(2, 1, 1)
    head.CanCollide = false
    head.Color = Color3.fromRGB(245, 225, 180)
    head.Material = Enum.Material.SmoothPlastic
    head.TopSurface = Enum.SurfaceType.Smooth
    head.BottomSurface = Enum.SurfaceType.Smooth
    head.Parent = npc

    local humanoid = Instance.new("Humanoid")
    humanoid.DisplayName = "NPC " .. tostring(index)
    humanoid.WalkSpeed = 14
    humanoid.JumpPower = 45
    humanoid.MaxHealth = 100
    humanoid.Health = 100
    humanoid.Parent = npc

    npc.PrimaryPart = root
    npc.Parent = mapFolder

    local angle = (index / math.max(totalNpcs, 1)) * math.pi * 2
    local spawnRadius = math.min(START_RADIUS * 0.35, 24)
    local spawnPosition = Vector3.new(
        CENTER.X + math.cos(angle) * spawnRadius,
        NPC_MOVE_HEIGHT,
        CENTER.Z + math.sin(angle) * spawnRadius
    )

    root.CFrame = CFrame.new(spawnPosition)
    head.CFrame = root.CFrame * CFrame.new(0, 1.5, 0)

    local weld = Instance.new("WeldConstraint")
    weld.Part0 = root
    weld.Part1 = head
    weld.Parent = root

    aliveNpcs[npc] = true
    table.insert(roundNpcs, npc)

    table.insert(activeConnections, humanoid.Died:Connect(function()
        aliveNpcs[npc] = false
    end))

    return npc
end

local function spawnSoloNpcsIfNeeded()
    if #Players:GetPlayers() ~= 1 then
        return
    end

    for index = 1, 2 do
        createSurvivorNpc(index, 2)
    end
end

local function startNpcBrainLoop()
    task.spawn(function()
        while roundActive do
            for _, npc in ipairs(roundNpcs) do
                if aliveNpcs[npc] then
                    local humanoid = npc:FindFirstChildOfClass("Humanoid")
                    local root = npc:FindFirstChild("HumanoidRootPart")

                    if humanoid and root and humanoid.Health > 0 then
                        local destination

                        if not isCharacterInsideCircle(npc) then
                            destination = Vector3.new(CENTER.X, NPC_MOVE_HEIGHT, CENTER.Z)
                        else
                            destination = getRandomPointInsideCircle(8)
                        end

                        local startPosition = root.Position
                        local flatDestination = Vector3.new(destination.X, NPC_MOVE_HEIGHT, destination.Z)
                        local moveDuration = math.random(20, 35) / 10
                        local moveStart = os.clock()

                        while roundActive and aliveNpcs[npc] and os.clock() - moveStart < moveDuration do
                            local alpha = math.clamp((os.clock() - moveStart) / moveDuration, 0, 1)
                            local nextPosition = startPosition:Lerp(flatDestination, alpha)
                            npc:PivotTo(CFrame.new(nextPosition, Vector3.new(flatDestination.X, nextPosition.Y, flatDestination.Z + 0.1)))
                            task.wait(0.05)
                        end
                    end
                end
            end

            task.wait(0.15)
        end
    end)
end

local function connectHazard(part)
    table.insert(activeConnections, part.Touched:Connect(function(hit)
        local character = hit:FindFirstAncestorOfClass("Model")
        if not character then
            return
        end

        local player = Players:GetPlayerFromCharacter(character)
        if player and alivePlayers[player] then
            eliminateCharacter(character)
            return
        end

        if aliveNpcs[character] then
            eliminateCharacter(character)
        end
    end))
end

local function createHazardPart(name, size, cframe, color)
    local part = Instance.new("Part")
    part.Name = name
    part.Anchored = true
    part.CanCollide = false
    part.CanTouch = true
    part.Size = size
    part.CFrame = cframe
    part.Material = Enum.Material.Neon
    part.Color = color
    part.Parent = hazardsFolder
    connectHazard(part)

    return part
end

local function runSweepingLaserEvent(duration)
    broadcast("Hard event: jump the sweeping lasers!", nil)

    local laserA = createHazardPart(
        "SweepingLaserA",
        Vector3.new(120, LASER_HIT_HEIGHT, 4),
        CFrame.new(CENTER.X, LASER_CENTER_HEIGHT, CENTER.Z - START_RADIUS),
        Color3.fromRGB(255, 35, 35)
    )

    local laserB = createHazardPart(
        "SweepingLaserB",
        Vector3.new(120, LASER_HIT_HEIGHT, 4),
        CFrame.new(CENTER.X, LASER_CENTER_HEIGHT, CENTER.Z + START_RADIUS),
        Color3.fromRGB(255, 35, 35)
    )

    local startTime = os.clock()
    while roundActive and os.clock() - startTime < duration do
        local alpha = (os.clock() - startTime) / duration
        local wave = math.sin(alpha * math.pi * 3)
        local offset = wave * (getSafeZoneRadius() - 4)

        laserA.CFrame = CFrame.new(CENTER.X, LASER_CENTER_HEIGHT, CENTER.Z + offset)
        laserB.CFrame = CFrame.new(CENTER.X, LASER_CENTER_HEIGHT, CENTER.Z - offset) * CFrame.Angles(0, math.rad(90), 0)
        task.wait(0.03)
    end

    laserA:Destroy()
    laserB:Destroy()
end

local function runRotatingLaserEvent(duration)
    broadcast("Hard event: rotating laser walls!", nil)

    local laser = createHazardPart(
        "RotatingLaserWall",
        Vector3.new(125, LASER_HIT_HEIGHT, 4),
        CFrame.new(CENTER.X, LASER_CENTER_HEIGHT, CENTER.Z),
        Color3.fromRGB(255, 70, 20)
    )

    local startTime = os.clock()
    while roundActive and os.clock() - startTime < duration do
        local rotation = (os.clock() - startTime) * math.rad(45)
        laser.CFrame = CFrame.new(CENTER.X, LASER_CENTER_HEIGHT, CENTER.Z) * CFrame.Angles(0, rotation, 0)
        task.wait(0.03)
    end

    laser:Destroy()
end

local function runMeteorEvent(duration)
    broadcast("Hard event: falling danger blocks!", nil)

    local startTime = os.clock()
    while roundActive and os.clock() - startTime < duration do
        local target = getRandomPointInsideCircle(8)
        local warning = createHazardPart(
            "MeteorWarning",
            Vector3.new(8, 0.2, 8),
            CFrame.new(target.X, 1.2, target.Z),
            Color3.fromRGB(255, 230, 60)
        )
        warning.Transparency = 0.25

        task.delay(1.8, function()
            if not roundActive or not warning.Parent then
                return
            end

            warning:Destroy()

            local meteor = createHazardPart(
                "DangerBlock",
                Vector3.new(9, 10, 9),
                CFrame.new(target.X, 5.5, target.Z),
                Color3.fromRGB(255, 40, 40)
            )

            task.delay(2.2, function()
                if meteor and meteor.Parent then
                    meteor:Destroy()
                end
            end)
        end)

        task.wait(2.2)
    end
end

local function runRandomEvent()
    local eventChoice = math.random(1, 8)

    if eventChoice == 1 then
        broadcast("Random event: speed boost!", nil)
        for _, player in ipairs(getAlivePlayers()) do
            local humanoid = player.Character and player.Character:FindFirstChildOfClass("Humanoid")
            if humanoid then
                humanoid.WalkSpeed = 26
            end
        end
        task.wait(14)
        for _, player in ipairs(getAlivePlayers()) do
            applyPlayerPerks(player)
        end
    elseif eventChoice == 2 then
        broadcast("Random event: low gravity!", nil)
        Workspace.Gravity = 75
        task.wait(14)
        Workspace.Gravity = currentGravity
    elseif eventChoice == 3 then
        broadcast("Random event: slippery floor!", nil)
        platform.CustomPhysicalProperties = PhysicalProperties.new(0.7, 0.02, 0.5)
        task.wait(14)
        platform.CustomPhysicalProperties = nil
    elseif eventChoice == 4 then
        broadcast("Random event: danger pulse!", nil)
        safeZone.Color = Color3.fromRGB(255, 80, 80)
        safeZone.Transparency = 0.25
        task.wait(10)
        safeZone.Color = Color3.fromRGB(0, 220, 150)
        safeZone.Transparency = 0.55
    elseif eventChoice == 5 then
        broadcast("Random event: tiny jumps!", nil)
        for _, player in ipairs(getAlivePlayers()) do
            local humanoid = player.Character and player.Character:FindFirstChildOfClass("Humanoid")
            if humanoid then
                humanoid.JumpPower = 20
            end
        end
        for npc, isAlive in pairs(aliveNpcs) do
            if isAlive then
                local humanoid = npc:FindFirstChildOfClass("Humanoid")
                if humanoid then
                    humanoid.JumpPower = 20
                end
            end
        end
        task.wait(14)
        for _, player in ipairs(getAlivePlayers()) do
            applyPlayerPerks(player)
        end
        for npc, isAlive in pairs(aliveNpcs) do
            if isAlive then
                local humanoid = npc:FindFirstChildOfClass("Humanoid")
                if humanoid then
                    humanoid.JumpPower = 45
                end
            end
        end
    elseif eventChoice == 6 then
        broadcast("Random event: slow motion!", nil)
        for _, player in ipairs(getAlivePlayers()) do
            local humanoid = player.Character and player.Character:FindFirstChildOfClass("Humanoid")
            if humanoid then
                humanoid.WalkSpeed = 8
            end
        end
        for npc, isAlive in pairs(aliveNpcs) do
            if isAlive then
                local humanoid = npc:FindFirstChildOfClass("Humanoid")
                if humanoid then
                    humanoid.WalkSpeed = 8
                end
            end
        end
        task.wait(14)
        for _, player in ipairs(getAlivePlayers()) do
            applyPlayerPerks(player)
        end
        for npc, isAlive in pairs(aliveNpcs) do
            if isAlive then
                local humanoid = npc:FindFirstChildOfClass("Humanoid")
                if humanoid then
                    humanoid.WalkSpeed = 14
                end
            end
        end
    elseif eventChoice == 7 then
        runSweepingLaserEvent(24)
    else
        if math.random(1, 2) == 1 then
            runRotatingLaserEvent(24)
        else
            runMeteorEvent(24)
        end
    end
end

local function startEliminationLoop()
    local connection = RunService.Heartbeat:Connect(function()
        if not roundActive then
            return
        end

        for _, player in ipairs(getAlivePlayers()) do
            eliminateIfOutside(player.Character)
            eliminateIfTouchingHazard(player.Character)
        end

        for npc, isAlive in pairs(aliveNpcs) do
            if isAlive then
                eliminateIfOutside(npc)
                eliminateIfTouchingHazard(npc)
            end
        end
    end)

    table.insert(activeConnections, connection)
end

local function startShrinkLoop()
    task.spawn(function()
        local shrinkTween = TweenService:Create(
            safeZone,
            TweenInfo.new(ROUND_TIME, Enum.EasingStyle.Linear, Enum.EasingDirection.Out),
            {
                Size = Vector3.new(SAFE_ZONE_HEIGHT, END_RADIUS * 2, END_RADIUS * 2),
                CFrame = CFrame.new(CENTER) * CFrame.Angles(0, 0, math.rad(90))
            }
        )

        shrinkSound:Play()
        shrinkTween:Play()
        shrinkTween.Completed:Wait()
    end)
end

local function waitForEnoughPlayers()
    while #Players:GetPlayers() < MIN_PLAYERS do
        broadcast("Waiting for at least 1 player...", nil)
        task.wait(1)
    end
end

local function runIntermission()
    for timeLeft = INTERMISSION_TIME, 1, -1 do
        if #Players:GetPlayers() < MIN_PLAYERS then
            return false
        end

        if #Players:GetPlayers() == 1 then
            broadcast("Solo round starts soon: 2 NPCs will join", timeLeft)
        else
            broadcast("Round starts soon", timeLeft)
        end

        task.wait(1)
    end

    return true
end

local function startRound()
    local players = Players:GetPlayers()
    if #players < MIN_PLAYERS then
        return
    end

    roundActive = true
    table.clear(alivePlayers)
    table.clear(aliveNpcs)
    clearRoundNpcs()
    currentGravity = Workspace.Gravity
    setSafeZoneRadius(START_RADIUS)
    safeZone.Color = Color3.fromRGB(0, 220, 150)
    safeZone.Transparency = 0.55

    for index, player in ipairs(players) do
        alivePlayers[player] = true
        teleportPlayerIntoCircle(player, index, #players)
        applyPlayerPerks(player)

        local character = player.Character or player.CharacterAdded:Wait()
        local humanoid = character:FindFirstChildOfClass("Humanoid")

        if humanoid then
            table.insert(activeConnections, humanoid.Died:Connect(function()
                alivePlayers[player] = false
                task.defer(function()
                    if roundActive and player.Parent == Players then
                        task.wait(1.5)
                        if roundActive and player.Parent == Players then
                            teleportPlayerToDeathArea(player)
                        end
                    end
                end)
            end))
        end
    end

    spawnSoloNpcsIfNeeded()

    startEliminationLoop()
    startNpcBrainLoop()
    startShrinkLoop()

    local nextRandomEventTime = os.clock() + math.random(8, 12)
    local roundStartTime = os.clock()
    local winner = nil

    while roundActive do
        local elapsed = os.clock() - roundStartTime
        local timeLeft = math.max(0, ROUND_TIME - elapsed)
        local survivors = getAliveContestants()

        broadcast("Stay inside the circle!", timeLeft)

        if #survivors <= 1 then
            winner = survivors[1]
            break
        end

        if timeLeft <= 0 then
            winner = survivors[math.random(1, #survivors)]
            break
        end

        if os.clock() >= nextRandomEventTime then
            task.spawn(runRandomEvent)
            nextRandomEventTime = os.clock() + math.random(18, 24)
        end

        task.wait(1)
    end

    roundActive = false

    if winner then
        if winner.Type == "Player" and winner.Player then
            local coins = getCoinsValue(winner.Player)
            local wins = getWinsValue(winner.Player)
            coins.Value += WIN_COINS
            wins.Value += 1
        end

        broadcast(winner.Name .. " wins the round!", 0)
    else
        broadcast("No winner this round", 0)
    end

    task.wait(5)
    resetRound()
end

Players.PlayerRemoving:Connect(function(player)
    alivePlayers[player] = nil
    playerPerks[player] = nil
end)

resetRound()

while true do
    waitForEnoughPlayers()

    if runIntermission() then
        startRound()
    end

    task.wait(2)
end
]]
serverScript.Parent = ServerScriptService

print("Last to Leave the Circle Wins installed successfully.")
print("Created: Workspace/LastToLeaveCircleMap")
print("Created: ServerScriptService/LastToLeaveCircleServer")
print("Created: ReplicatedStorage/LastToLeaveCircleUpdate")
print("Created: ReplicatedStorage/LastToLeaveCircleShop")
print("Created: StarterGui/LastToLeaveCircleGui")
print("Press Play with 1 player to test against 2 NPCs, or with 2+ players for multiplayer.")
