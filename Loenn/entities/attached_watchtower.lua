local a_watchtower = {}

a_watchtower.name = "PrismaticHelper/AttachedWatchtower"
a_watchtower.depth = -8500
a_watchtower.justification = { 0.5, 1.0 }
a_watchtower.nodeLineRenderType = "line"
a_watchtower.texture = "objects/lookout/lookout05"
a_watchtower.nodeLimits = { 0, -1 }
a_watchtower.placements = {
    name = "attached_watchtower",
    data = {
        summit = false,
        onlyY = false
    }
}

--return a_watchtower
return nil