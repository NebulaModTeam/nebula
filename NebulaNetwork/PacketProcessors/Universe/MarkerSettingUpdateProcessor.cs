#region

using System.Linq;
using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Universe;

[RegisterPacketProcessor]
internal class MarkerSettingUpdateProcessor : PacketProcessor<MarkerSettingUpdatePacket>
{
    // Cache for the Marker owner type (discovered via reflection)
    private static object _markerOwnerType;
    private static bool _markerOwnerTypeDiscovered;

    /// <summary>
    /// Creates a TodoModule for a marker using galacticDigital.AddTodoModule().
    /// This properly registers the todo in the global pool.
    /// </summary>
    private static TodoModule CreateMarkerTodoModule(int markerId)
    {
        var galacticDigital = GameMain.data?.galacticDigital;
        if (galacticDigital == null)
        {
            Log.Warn("CreateMarkerTodoModule: galacticDigital is null");
            return null;
        }

        try
        {
            var bindingFlags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            var addTodoMethod = galacticDigital.GetType().GetMethod("AddTodoModule", bindingFlags);

            if (addTodoMethod == null)
            {
                Log.Warn("CreateMarkerTodoModule: AddTodoModule method not found");
                return null;
            }

            // Discover the Marker owner type if not already cached
            if (!_markerOwnerTypeDiscovered)
            {
                var ownerTypeEnum = addTodoMethod.GetParameters()[0].ParameterType;
                var enumValues = System.Enum.GetValues(ownerTypeEnum);

                // Log all enum values for debugging
                Log.Debug($"CreateMarkerTodoModule: ETodoModuleOwnerType values: {string.Join(", ", enumValues.Cast<object>().Select(v => $"{v}={System.Convert.ToInt32(v)}"))}");

                // Search for "Entity" type in the enum (markers use ETodoModuleOwnerType.Entity = 2)
                foreach (var val in enumValues)
                {
                    var name = val.ToString();
                    if (name == "Entity")
                    {
                        _markerOwnerType = val;
                        Log.Debug($"CreateMarkerTodoModule: Found Entity owner type: {val}");
                        break;
                    }
                }

                // If not found, log warning but don't fail
                if (_markerOwnerType == null)
                {
                    Log.Warn("CreateMarkerTodoModule: Could not find Entity owner type in ETodoModuleOwnerType enum");
                }

                _markerOwnerTypeDiscovered = true;
            }

            if (_markerOwnerType == null)
            {
                return null;
            }

            // Call AddTodoModule(markerOwnerType, markerId)
            Log.Debug($"CreateMarkerTodoModule: Calling AddTodoModule({_markerOwnerType}, {markerId})");
            var result = addTodoMethod.Invoke(galacticDigital, new object[] { _markerOwnerType, markerId });

            if (result is TodoModule todoModule)
            {
                Log.Debug($"CreateMarkerTodoModule: Created TodoModule for marker {markerId}");
                return todoModule;
            }

            Log.Warn($"CreateMarkerTodoModule: AddTodoModule returned {result?.GetType().Name ?? "null"} instead of TodoModule");
            return null;
        }
        catch (System.Exception ex)
        {
            Log.Warn($"CreateMarkerTodoModule: Exception - {ex.Message}\n{ex.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// Finds an existing TodoModule for a marker in the galactic todos pool.
    /// </summary>
    private static TodoModule FindMarkerTodoInPool(int markerId)
    {
        var todos = GameMain.data?.galacticDigital?.todos;
        if (todos == null) return null;

        for (int i = 1; i < todos.cursor; i++)
        {
            ref var todo = ref todos.buffer[i];
            if (todo.id == i && todo.ownerId == markerId)
            {
                // Verify it's a marker type (not planet or other)
                if (_markerOwnerType != null && System.Convert.ToInt32(todo.ownerType) == System.Convert.ToInt32(_markerOwnerType))
                {
                    return todo;
                }
            }
        }

        return null;
    }

    protected override void ProcessPacket(MarkerSettingUpdatePacket packet, NebulaConnection conn)
    {
        Log.Debug($"MarkerSettingUpdatePacket received: Planet={packet.PlanetId}, Marker={packet.MarkerId}, Event={packet.Event}");

        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        if (factory == null)
        {
            Log.Warn($"MarkerSettingUpdatePacket: Can't find factory for planet {packet.PlanetId}");
            return;
        }

        var markers = factory.digitalSystem?.markers;
        if (markers == null || packet.MarkerId <= 0 || packet.MarkerId >= markers.cursor)
        {
            Log.Warn($"MarkerSettingUpdatePacket: Can't find marker ({packet.PlanetId}, {packet.MarkerId}), cursor={markers?.cursor}");
            return;
        }

        ref var marker = ref markers.buffer[packet.MarkerId];
        if (marker.id != packet.MarkerId)
        {
            Log.Warn($"MarkerSettingUpdatePacket: Marker id mismatch ({packet.PlanetId}, {packet.MarkerId})");
            return;
        }

        if (IsHost)
        {
            // Relay packet to other clients
            Server.SendPacketExclude(packet, conn);
        }

        using (Multiplayer.Session.Warning.IsIncomingMarkerPacket.On())
        {
            switch (packet.Event)
            {
                case MarkerSettingEvent.SetName:
                    marker.name = packet.StringValue;
                    break;

                case MarkerSettingEvent.SetWord:
                    marker.word = packet.StringValue;
                    break;

                case MarkerSettingEvent.SetTags:
                    marker.tags = packet.StringValue;
                    break;

                case MarkerSettingEvent.SetTodoContent:
                    if (marker.todo != null)
                    {
                        marker.todo.content = packet.StringValue;
                        // Apply color data if provided (colors and text are sent together)
                        if (packet.ColorData != null)
                        {
                            marker.todo.contentColorIndex = packet.ColorData;
                        }
                        Log.Debug($"SetTodoContent: Updated todo for marker {packet.MarkerId}, colors={packet.ColorData?.Length ?? 0}");
                    }
                    else
                    {
                        // marker.todo is null - create via galacticDigital.AddTodoModule() for proper registration
                        Log.Debug($"SetTodoContent: marker.todo is null for marker {packet.MarkerId}, creating via AddTodoModule");

                        // First check if there's already a todo in the pool for this marker
                        var existingTodo = FindMarkerTodoInPool(packet.MarkerId);
                        if (existingTodo != null)
                        {
                            // Link existing pool todo to marker and update content
                            marker.todo = existingTodo;
                            marker.todo.content = packet.StringValue;
                            if (packet.ColorData != null)
                            {
                                marker.todo.contentColorIndex = packet.ColorData;
                            }
                            Log.Debug($"SetTodoContent: Found existing todo in pool, linked to marker {packet.MarkerId}");
                        }
                        else
                        {
                            // Create new todo via AddTodoModule
                            var newTodo = CreateMarkerTodoModule(packet.MarkerId);
                            if (newTodo != null)
                            {
                                marker.todo = newTodo;
                                marker.todo.content = packet.StringValue;
                                if (packet.ColorData != null)
                                {
                                    marker.todo.contentColorIndex = packet.ColorData;
                                }
                                Log.Debug($"SetTodoContent: Created and linked TodoModule for marker {packet.MarkerId}");
                            }
                            else
                            {
                                Log.Warn($"SetTodoContent: Failed to create TodoModule for marker {packet.MarkerId}");
                            }
                        }
                    }
                    break;

                case MarkerSettingEvent.SetIcon:
                    marker.icon = packet.IntValue;
                    break;

                case MarkerSettingEvent.SetColor:
                    marker.color = (byte)packet.IntValue;
                    break;

                case MarkerSettingEvent.SetVisibility:
                    marker.visibility = (EMarkerVisibility)packet.IntValue;
                    break;

                case MarkerSettingEvent.SetDetailLevel:
                    marker.detailLevel = (EMarkerDetailLevel)packet.IntValue;
                    break;

                case MarkerSettingEvent.SetHeight:
                    marker.SetHeight(factory.entityPool, packet.FloatValue);
                    break;

                case MarkerSettingEvent.SetRadius:
                    marker.SetRadius(packet.FloatValue);
                    break;

                default:
                    Log.Warn($"MarkerSettingUpdatePacket: Unknown MarkerSettingEvent {packet.Event}");
                    break;
            }

            Log.Debug($"MarkerSettingUpdatePacket applied: Event={packet.Event}, icon={marker.icon}, color={marker.color}, name='{marker.name}'");

            // Trigger visual update on the marker
            try
            {
                marker.InternalUpdate();
            }
            catch (System.Exception)
            {
                // InternalUpdate may fail if marker visuals aren't fully initialized yet
            }
        }
    }
}
