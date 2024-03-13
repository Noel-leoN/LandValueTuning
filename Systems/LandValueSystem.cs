using System.Runtime.CompilerServices;
using Game;
using Game.Areas;
using Game.Buildings;
using Game.Citizens;
using Game.Common;
using Game.Economy;
using Game.Net;
using Game.Objects;
using Game.Prefabs;
using Game.Simulation;
using Game.Tools;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Scripting;

namespace LandValueTuning.Systems;


[CompilerGenerated]
public class LandValueSystemMod : GameSystemBase
{
    [BurstCompile]
    private struct EdgeUpdateJob : IJobChunk
    {
        [ReadOnly]
        public EntityTypeHandle m_EntityType;

        [ReadOnly]
        public BufferTypeHandle<ConnectedBuilding> m_ConnectedBuildingType;

        [ReadOnly]
        public ComponentTypeHandle<Edge> m_EdgeType;

        [ReadOnly]
        public ComponentTypeHandle<Curve> m_CurveType;

        [NativeDisableParallelForRestriction]
        public ComponentLookup<LandValue> m_LandValues;

        [ReadOnly]
        public BufferLookup<Renter> m_RenterBuffers;

        [ReadOnly]
        public ComponentLookup<Game.Objects.Transform> m_Transforms;

        [ReadOnly]
        public ComponentLookup<PropertyRenter> m_PropertyRenters;

        [ReadOnly]
        public ComponentLookup<PrefabRef> m_Prefabs;

        [ReadOnly]
        public ComponentLookup<BuildingData> m_BuildingDatas;

        [ReadOnly]
        public ComponentLookup<Abandoned> m_Abandoneds;

        [ReadOnly]
        public ComponentLookup<Destroyed> m_Destroyeds;

        [ReadOnly]
        public ComponentLookup<ConsumptionData> m_ConsumptionDatas;

        [ReadOnly]
        public ComponentLookup<BuildingPropertyData> m_PropertyDatas;

        [ReadOnly]
        public ComponentLookup<Household> m_Households;

        [ReadOnly]
        public ComponentLookup<Placeholder> m_Placeholders;

        [ReadOnly]
        public ComponentLookup<Attached> m_Attached;

        [ReadOnly]
        public BufferLookup<Game.Areas.SubArea> m_SubAreas;

        [ReadOnly]
        public ComponentLookup<Game.Areas.Lot> m_Lots;

        [ReadOnly]
        public ComponentLookup<Geometry> m_Geometries;

        [ReadOnly]
        public NativeArray<GroundPollution> m_PollutionMap;

        [ReadOnly]
        public PollutionParameterData m_PollutionParameters;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            NativeArray<Entity> nativeArray;
            nativeArray = chunk.GetNativeArray(m_EntityType);
            NativeArray<Edge> nativeArray2;
            nativeArray2 = chunk.GetNativeArray(ref m_EdgeType);
            NativeArray<Curve> nativeArray3;
            nativeArray3 = chunk.GetNativeArray(ref m_CurveType);
            BufferAccessor<ConnectedBuilding> bufferAccessor;
            //�μ�ConnectedBuildingSystem��
            bufferAccessor = chunk.GetBufferAccessor(ref m_ConnectedBuildingType);
            for (int i = 0; i < nativeArray2.Length; i++)
            
            {
                Entity entity;
                entity = nativeArray[i];
                Entity start;
                start = nativeArray2[i].m_Start;
                Entity end;
                end = nativeArray2[i].m_End;
                LandValue value;
                value = m_LandValues[entity];
                int num;
                num = 0;
                float num2;
                num2 = 0f;
                int num3;
                num3 = 0;
                float num4;
                num4 = 0f;
                DynamicBuffer<ConnectedBuilding> dynamicBuffer;
                
                dynamicBuffer = bufferAccessor[i];
                for (int j = 0; j < dynamicBuffer.Length; j++)
               
                {
                    Entity building;
                    building = dynamicBuffer[j].m_Building;
                   
                    if (m_Prefabs.HasComponent(building) && !m_Placeholders.HasComponent(building))
                    {
                        Entity prefab;
                        prefab = m_Prefabs[building].m_Prefab;
                      
                        if (!m_PropertyDatas.HasComponent(prefab) || m_Abandoneds.HasComponent(building) || m_Destroyeds.HasComponent(building))
                        {
                            continue;
                        }
                        BuildingPropertyData buildingPropertyData;
                        buildingPropertyData = m_PropertyDatas[prefab];
                       
                        if (buildingPropertyData.m_AllowedStored != Resource.NoResource)
                        {
                            continue;
                        }
                        BuildingData buildingData;
                        buildingData = m_BuildingDatas[prefab];
                        ConsumptionData consumptionData;
                        consumptionData = m_ConsumptionDatas[prefab];
                        int num5;
                        num5 = buildingPropertyData.CountProperties();
                        bool flag;
                        flag = buildingPropertyData.m_ResidentialProperties > 0 && (buildingPropertyData.m_AllowedSold != Resource.NoResource || buildingPropertyData.m_AllowedManufactured != Resource.NoResource);
                        int num6;
                        num6 = buildingData.m_LotSize.x * buildingData.m_LotSize.y;
                        
                        if (m_Attached.HasComponent(building))
                        {
                            Entity parent;
                            parent = m_Attached[building].m_Parent;
                            if (m_SubAreas.HasBuffer(parent))
                            {
                                num6 += Mathf.CeilToInt(ExtractorAISystem.GetArea(m_SubAreas[parent], m_Lots, m_Geometries));
                            }
                        }
                        float num7;
                        num7 = value.m_LandValue * num6 / math.max(1, num5);
                        float num8;
                        num8 = consumptionData.m_Upkeep / (float)math.max(1, num5);
                        //***
                        if (m_RenterBuffers.HasBuffer(building))
                        {
                            DynamicBuffer<Renter> dynamicBuffer2;
                            dynamicBuffer2 = m_RenterBuffers[building];
                            
                            for (int k = 0; k < dynamicBuffer2.Length; k++)
                            {
                                Entity renter;
                                renter = dynamicBuffer2[k].m_Renter;
                                if (m_PropertyRenters.HasComponent(renter))
                                {
                                    PropertyRenter propertyRenter;
                                    propertyRenter = m_PropertyRenters[renter];
                                    //num2���µؼ�����:����������-ά���ѣ���������3����ǰ�ؼ�������1����֮����1��
                                    //���л�Ͻ���ά���Ѻ͵ؼ۰�0.4����,��������0.4���������㣻
                                    //����ÿ��num2�õ��ۼ�ֵ��
                                    //New feature to calculate landvalue separately for each zonetype by simply change the result;
                                    //method may extened by cap the max landvalue; 
                                    //Avoid to modifiy MaxRent for unpredictable company profits;
                                    float resmixf = 1f;//ס��ס�����;
                                    if (!flag || m_Households.HasComponent(renter))
                                    {
                                        //num2 = (float)(num2 + (propertyRenter.m_MaxRent - num8 >= 3f * num7 ? 1f : -1f));
                                        float residentlowf = 0.5f;//����סլ��1����
                                        float residenthighf = 0.8f;//�и���סլ;(����ס��ס�����);
                                        float commerialf = 0.8f;//��ҵ��
                                        float manufacturf = 1f;//����ҵ��
                                        float officef = 0.3f;//�칫��
                                        float extractorf = 1f;//�ɼ�ҵ��                                        
                                        //float storagef = 1f;//�ִ�ҵ��
                                        //����סլ���ʲ�����ֵΪ1��;���������Ƶؼ��������öȣ�
                                        if (buildingPropertyData.m_ResidentialProperties == 1)
                                        {
                                            num2 = (float)(num2 + ((propertyRenter.m_MaxRent - num8 >= 3f * num7) ? 1f : (-1f)));
                                            num2 *= residentlowf;
                                        }
                                        //�и���סլ������ס�̻죩����΢���������Ƹ����סլ�ؼۣ�
                                        if (buildingPropertyData.m_ResidentialProperties > 1 && (buildingPropertyData.m_AllowedSold == Resource.NoResource || buildingPropertyData.m_AllowedManufactured == Resource.NoResource))
                                        {
                                            num2 = ((float)(num2 + (((float)propertyRenter.m_MaxRent - num8 >= 3f * num7) ? 1f : (-1f))));
                                            num2 *= residenthighf;
                                        }
                                        //��ҵ����΢���������Ƹ�������ҵ�ؼۣ�
                                        if (buildingPropertyData.m_AllowedSold != Resource.NoResource && buildingPropertyData.m_ResidentialProperties <= 0)
                                        {
                                            num2 = ((float)(num2 + (((float)propertyRenter.m_MaxRent - num8 >= 3f * num7) ? 1f : (-1f))));
                                            num2 *= commerialf;
                                        }
                                        //��ҵ��
                                        if (buildingPropertyData.m_AllowedManufactured != Resource.NoResource && buildingPropertyData.m_ResidentialProperties <= 0)
                                        {
                                            //IndustrialProcessData process = this.m_IndustrialProcessDatas[prefab];
                                            //����ҵ���ؼ�����ȾӰ��󣬽�������������ͣ�
                                            if (!EconomyUtils.IsOfficeResource(buildingPropertyData.m_AllowedManufactured) && !EconomyUtils.IsExtractorResource(buildingPropertyData.m_AllowedManufactured) && buildingPropertyData.m_AllowedStored == Resource.NoResource)
                                                num2 = ((float)(num2 + (((float)propertyRenter.m_MaxRent - num8 >= 3f * num7) ? 1f : (-1f))));
                                            num2 *= manufacturf;
                                            //�칫��������������Ƴ�������칫�ؼۣ�may not suitable for RealEco��                                            
                                            if (EconomyUtils.IsOfficeResource(buildingPropertyData.m_AllowedManufactured))
                                            {
                                                num2 = ((float)(num2 + (((float)propertyRenter.m_MaxRent - num8 >= 3f * num7) ? 1f : (-1f))));
                                                num2 *= officef;
                                            }
                                            //�ɼ�ҵ���ؼ�Ӱ�첻��
                                            if (EconomyUtils.IsExtractorResource(buildingPropertyData.m_AllowedManufactured))
                                            {
                                                num2 = ((float)(num2 + (((float)propertyRenter.m_MaxRent - num8 >= 3f * num7) ? 1f : (-1f))));
                                                num2 *= extractorf;
                                            }

                                        }
                                    }
                                    else//ס��ס����ϣ�
                                    {
                                        num2 = (float)(num2 + (propertyRenter.m_MaxRent - RentAdjustSystem.kMixedCompanyRent * consumptionData.m_Upkeep >= 3f * RentAdjustSystem.kMixedCompanyRent * value.m_LandValue ? Mathf.RoundToInt(RentAdjustSystem.kMixedCompanyRent * buildingPropertyData.m_ResidentialProperties) : -Mathf.RoundToInt(RentAdjustSystem.kMixedCompanyRent * buildingPropertyData.m_ResidentialProperties)));
                                        num2 *= resmixf;
                                    }
                                    //***�����⻧����num3��
                                    num3++;
                                }
                            }
                            num += num6;
                            int num9;
                            num9 = num5 - dynamicBuffer2.Length;
                            num2 -= num9;
                            num3 += num9;
                        }
                        
                        if (m_Transforms.HasComponent(building))
                        {
                            num4 = GroundPollutionSystem.GetPollution(m_Transforms[building].m_Position, m_PollutionMap).m_Pollution / (float)m_PollutionParameters.m_GroundPollutionLandValueDivisor;
                        }
                    }
                   
                    else
                    {
                        num++;
                    }
                    //ѭ���ۼƽ����
                    //numΪ��ǰ��·edge���н�����ռ�����������(���Գ���Ϊ1���Գ���ʵ��);
                    //num2Ϊ��ǰ��·edge���н����ؼ��������ۼ�ֵ(���п����ʲ���ÿ1����1);
                    //num3Ϊ��ǰ��·edge���н����Ŀɳ����ܻ���(��˾1����1��סլ����������);
                }
                float length;
                length = nativeArray3[i].m_Length;
                float distanceFade;
                distanceFade = LandValueSystemMod.GetDistanceFade(length);


                //***Fix method***//
                //int num10;//vanilla
                //num10 = num;//vanilla//��·edge��������������;                
                //num = math.max(num, Mathf.CeilToInt(length / 4f));//vanilla//num=��·����1/4�����������нϴ�ֵ;                
                //num��Ӱ���·�ؼ�Ȩ�أ�����·����1/4��num�Ƚϣ���������Ȩ��ֵ��num��С���򲻱�;
                //***notice***//
                //num3 -= num - num10;//vanilla//num3=·��edge�����ܻ���-����·����1/4����-��·����������;
                //The vanilla method seems to calculate the impact factor of dense or sparse buildings on the road;
                //but it seems to be going in the wrong direction;                
                //����ϡ�赼����ߵؼۣ�ԭ���ڵ���·����1/4�����н����ĵ�·��������ʱ(���������ϡ���ɢ)��ʹnum3���٣��⽫Ӱ��ؼ�������num12=0.1*num2/num3�ڵ�·�ر�ʱ(num3��С)����仯��������޽����ĳ�����·Ī�������ߵؼ�;
                //�����ܼ�����֮��Ȼ;               
                
                int num10 = math.max(num, Mathf.CeilToInt(length / 4f));//new fix;
                num3 -= num - num10;//new fix:��num10������num�Ե�,num���ֲ��䣻
                                    //�޸ĺ�num3��·�ܻ�������=�ܻ���-����·��������-��·����1/4);
                                    //�ܼ��ضΣ���·������������·�γ���1/4�����·�ܻ������Ӽ��٣��Ӷ��ؼ�������num12=0.1*num2/num3����ֵ���ӣ�
                                    //ϡ��ضΣ���֮��Ȼ��

                float2 @float;
                
                @float = new float2(math.max(1f, m_LandValues[start].m_Weight), math.max(1f, m_LandValues[end].m_Weight));
                float num11;
                num11 = @float.x + @float.y;
               
                float2 float2;
                float2 = new float2(m_LandValues[start].m_LandValue, m_LandValues[end].m_LandValue);
                
                @float *= distanceFade;
                                         
                float y;
                y = math.lerp(float2.x, float2.y, @float.y / num11);
                
                if (num4 > 0f)
                {
                    num4 = math.lerp(0f, 2f, num4 / 50f);
                }

                //��Ӧǰ��num fix��                
                //value.m_Weight = math.max(1f, math.lerp(value.m_Weight, num, 0.1f));//vanilla;
                value.m_Weight = math.max(1f, math.lerp(value.m_Weight, num10, 0.1f));
                float s;
                s = num11 / (99f * value.m_Weight + num11);
               
                value.m_LandValue = math.lerp(value.m_LandValue, y, s);
               


                float num12;
                num12 = 0f;
                //��·���Գ���������>0����ؼ�������=0.1 * ���н����ؼ��������ܺ�/������������(����Ӱ�죩��
                if (num3 > 0)
                {
                    num12 = 0.1f * num2 / num3;
                }
                if (value.m_LandValue > 30f)//��Ⱦ���أ����ؼ۴���30��Ч��
                {
                    num12 -= num4 * 0.2f;
                }
                //���º��·�ؼ�=ԭ�ؼ�+num12������������-2С��1��
                value.m_LandValue += math.min(1f, math.max(-2f, num12));
                //���º�ؼ۲���С��0������Ϊ0��
                value.m_LandValue = math.max(value.m_LandValue, 0f);
                //���º�ؼ�Ȩ��(����ǿ��)��
                value.m_Weight = math.lerp(value.m_Weight, math.max(1f, 0.5f * num11), s);
                
                m_LandValues[entity] = value;

                
            }
        }

        void IJobChunk.Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            Execute(in chunk, unfilteredChunkIndex, useEnabledMask, in chunkEnabledMask);
        }
    }

    [BurstCompile]
    private struct NodeUpdateJob : IJobChunk
    {
        [ReadOnly]
        public EntityTypeHandle m_EntityType;

        [ReadOnly]
        public ComponentTypeHandle<Game.Net.Node> m_NodeType;

        [ReadOnly]
        public BufferTypeHandle<ConnectedEdge> m_ConnectedEdgeType;

        [NativeDisableParallelForRestriction]
        public ComponentLookup<LandValue> m_LandValues;

        [ReadOnly]
        public ComponentLookup<Curve> m_Curves;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            NativeArray<Entity> nativeArray;
            nativeArray = chunk.GetNativeArray(m_EntityType);
            NativeArray<Game.Net.Node> nativeArray2;
            nativeArray2 = chunk.GetNativeArray(ref m_NodeType);
            BufferAccessor<ConnectedEdge> bufferAccessor;
            bufferAccessor = chunk.GetBufferAccessor(ref m_ConnectedEdgeType);
            for (int i = 0; i < nativeArray2.Length; i++)
            {
                Entity entity;
                entity = nativeArray[i];
                float num;
                num = 0f;
                float num2;
                num2 = 0f;
                DynamicBuffer<ConnectedEdge> dynamicBuffer;
                dynamicBuffer = bufferAccessor[i];
                for (int j = 0; j < dynamicBuffer.Length; j++)
                {
                    Entity edge;
                    edge = dynamicBuffer[j].m_Edge;
                    if (m_LandValues.HasComponent(edge))
                    {
                        float landValue;
                        landValue = m_LandValues[edge].m_LandValue;
                        float num3;
                        num3 = m_LandValues[edge].m_Weight;
                        if (m_Curves.HasComponent(edge))
                        {
                            num3 *= LandValueSystemMod.GetDistanceFade(m_Curves[edge].m_Length);
                        }
                        
                            num += landValue * num3;
                            num2 += num3;
                         
                    }
                }
                
                if (num2 != 0f)
                {
                    num /= num2;                    
                    LandValue value;
                    value = m_LandValues[entity];
                    value.m_LandValue = math.lerp(value.m_LandValue, num, 0.05f);
                    
                    value.m_Weight = math.max(1f, math.lerp(value.m_Weight, num2 / dynamicBuffer.Length, 0.05f));
                    m_LandValues[entity] = value;
                }
                
            }
        }

        void IJobChunk.Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            Execute(in chunk, unfilteredChunkIndex, useEnabledMask, in chunkEnabledMask);
        }
    }

    private struct TypeHandle
    {
        [ReadOnly]
        public EntityTypeHandle __Unity_Entities_Entity_TypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<Edge> __Game_Net_Edge_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<Curve> __Game_Net_Curve_RO_ComponentTypeHandle;

        [ReadOnly]
        public BufferTypeHandle<ConnectedBuilding> __Game_Buildings_ConnectedBuilding_RO_BufferTypeHandle;

        [ReadOnly]
        public ComponentLookup<BuildingData> __Game_Prefabs_BuildingData_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Game.Objects.Transform> __Game_Objects_Transform_RO_ComponentLookup;

        public ComponentLookup<LandValue> __Game_Net_LandValue_RW_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<PrefabRef> __Game_Prefabs_PrefabRef_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<PropertyRenter> __Game_Buildings_PropertyRenter_RO_ComponentLookup;

        [ReadOnly]
        public BufferLookup<Renter> __Game_Buildings_Renter_RO_BufferLookup;

        [ReadOnly]
        public ComponentLookup<Abandoned> __Game_Buildings_Abandoned_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Destroyed> __Game_Common_Destroyed_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<ConsumptionData> __Game_Prefabs_ConsumptionData_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<BuildingPropertyData> __Game_Prefabs_BuildingPropertyData_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Household> __Game_Citizens_Household_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Placeholder> __Game_Objects_Placeholder_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Attached> __Game_Objects_Attached_RO_ComponentLookup;

        [ReadOnly]
        public BufferLookup<Game.Areas.SubArea> __Game_Areas_SubArea_RO_BufferLookup;

        [ReadOnly]
        public ComponentLookup<Game.Areas.Lot> __Game_Areas_Lot_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Geometry> __Game_Areas_Geometry_RO_ComponentLookup;

        [ReadOnly]
        public ComponentTypeHandle<Game.Net.Node> __Game_Net_Node_RO_ComponentTypeHandle;

        [ReadOnly]
        public BufferTypeHandle<ConnectedEdge> __Game_Net_ConnectedEdge_RO_BufferTypeHandle;

        [ReadOnly]
        public ComponentLookup<Curve> __Game_Net_Curve_RO_ComponentLookup;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void __AssignHandles(ref SystemState state)
        {
            __Unity_Entities_Entity_TypeHandle = state.GetEntityTypeHandle();
            __Game_Net_Edge_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Edge>(isReadOnly: true);
            __Game_Net_Curve_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Curve>(isReadOnly: true);
            __Game_Buildings_ConnectedBuilding_RO_BufferTypeHandle = state.GetBufferTypeHandle<ConnectedBuilding>(isReadOnly: true);
            __Game_Prefabs_BuildingData_RO_ComponentLookup = state.GetComponentLookup<BuildingData>(isReadOnly: true);
            __Game_Objects_Transform_RO_ComponentLookup = state.GetComponentLookup<Game.Objects.Transform>(isReadOnly: true);
            __Game_Net_LandValue_RW_ComponentLookup = state.GetComponentLookup<LandValue>();
            __Game_Prefabs_PrefabRef_RO_ComponentLookup = state.GetComponentLookup<PrefabRef>(isReadOnly: true);
            __Game_Buildings_PropertyRenter_RO_ComponentLookup = state.GetComponentLookup<PropertyRenter>(isReadOnly: true);
            __Game_Buildings_Renter_RO_BufferLookup = state.GetBufferLookup<Renter>(isReadOnly: true);
            __Game_Buildings_Abandoned_RO_ComponentLookup = state.GetComponentLookup<Abandoned>(isReadOnly: true);
            __Game_Common_Destroyed_RO_ComponentLookup = state.GetComponentLookup<Destroyed>(isReadOnly: true);
            __Game_Prefabs_ConsumptionData_RO_ComponentLookup = state.GetComponentLookup<ConsumptionData>(isReadOnly: true);
            __Game_Prefabs_BuildingPropertyData_RO_ComponentLookup = state.GetComponentLookup<BuildingPropertyData>(isReadOnly: true);
            __Game_Citizens_Household_RO_ComponentLookup = state.GetComponentLookup<Household>(isReadOnly: true);
            __Game_Objects_Placeholder_RO_ComponentLookup = state.GetComponentLookup<Placeholder>(isReadOnly: true);
            __Game_Objects_Attached_RO_ComponentLookup = state.GetComponentLookup<Attached>(isReadOnly: true);
            __Game_Areas_SubArea_RO_BufferLookup = state.GetBufferLookup<Game.Areas.SubArea>(isReadOnly: true);
            __Game_Areas_Lot_RO_ComponentLookup = state.GetComponentLookup<Game.Areas.Lot>(isReadOnly: true);
            __Game_Areas_Geometry_RO_ComponentLookup = state.GetComponentLookup<Geometry>(isReadOnly: true);
            __Game_Net_Node_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Game.Net.Node>(isReadOnly: true);
            __Game_Net_ConnectedEdge_RO_BufferTypeHandle = state.GetBufferTypeHandle<ConnectedEdge>(isReadOnly: true);
            __Game_Net_Curve_RO_ComponentLookup = state.GetComponentLookup<Curve>(isReadOnly: true);
        }
    }

    private GroundPollutionSystem m_GroundPollutionSystem;

    private EntityQuery m_EdgeGroup;

    private EntityQuery m_NodeGroup;

    private EntityQuery m_PollutionParameterQuery;

    private TypeHandle __TypeHandle;

    public override int GetUpdateInterval(SystemUpdatePhase phase)
    {
        return 16;
    }

    [Preserve]
    protected override void OnCreate()
    {
        base.OnCreate();
        m_GroundPollutionSystem = World.GetOrCreateSystemManaged<GroundPollutionSystem>();
        m_PollutionParameterQuery = GetEntityQuery(ComponentType.ReadOnly<PollutionParameterData>());
        m_EdgeGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[4]
            {
            ComponentType.ReadOnly<Edge>(),
            ComponentType.ReadWrite<LandValue>(),
            ComponentType.ReadOnly<Curve>(),
            ComponentType.ReadOnly<ConnectedBuilding>()
            },
            Any = new ComponentType[0],
            None = new ComponentType[2]
            {
            ComponentType.ReadOnly<Deleted>(),
            ComponentType.ReadOnly<Temp>()
            }
        });
        m_NodeGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[3]
            {
            ComponentType.ReadOnly<Game.Net.Node>(),
            ComponentType.ReadWrite<LandValue>(),
            ComponentType.ReadOnly<ConnectedEdge>()
            },
            Any = new ComponentType[0],
            None = new ComponentType[2]
            {
            ComponentType.ReadOnly<Deleted>(),
            ComponentType.ReadOnly<Temp>()
            }
        });
        RequireAnyForUpdate(m_EdgeGroup, m_NodeGroup);
    }

    [Preserve]
    protected override void OnUpdate()
    {
        JobHandle jobHandle;
        jobHandle = Dependency;
        if (!m_EdgeGroup.IsEmptyIgnoreFilter)
        {
            __TypeHandle.__Game_Areas_Geometry_RO_ComponentLookup.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Areas_Lot_RO_ComponentLookup.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Areas_SubArea_RO_BufferLookup.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Objects_Attached_RO_ComponentLookup.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Objects_Placeholder_RO_ComponentLookup.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Citizens_Household_RO_ComponentLookup.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Prefabs_ConsumptionData_RO_ComponentLookup.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Common_Destroyed_RO_ComponentLookup.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Buildings_Abandoned_RO_ComponentLookup.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Buildings_Renter_RO_BufferLookup.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentLookup.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentLookup.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Net_LandValue_RW_ComponentLookup.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Objects_Transform_RO_ComponentLookup.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Prefabs_BuildingData_RO_ComponentLookup.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Buildings_ConnectedBuilding_RO_BufferTypeHandle.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Net_Curve_RO_ComponentTypeHandle.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Net_Edge_RO_ComponentTypeHandle.Update(ref CheckedStateRef);
            __TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref CheckedStateRef);
            EdgeUpdateJob jobData;
            jobData = default;
            jobData.m_EntityType = __TypeHandle.__Unity_Entities_Entity_TypeHandle;
            jobData.m_EdgeType = __TypeHandle.__Game_Net_Edge_RO_ComponentTypeHandle;
            jobData.m_CurveType = __TypeHandle.__Game_Net_Curve_RO_ComponentTypeHandle;
            jobData.m_ConnectedBuildingType = __TypeHandle.__Game_Buildings_ConnectedBuilding_RO_BufferTypeHandle;
            jobData.m_BuildingDatas = __TypeHandle.__Game_Prefabs_BuildingData_RO_ComponentLookup;
            jobData.m_Transforms = __TypeHandle.__Game_Objects_Transform_RO_ComponentLookup;
            jobData.m_LandValues = __TypeHandle.__Game_Net_LandValue_RW_ComponentLookup;
            jobData.m_Prefabs = __TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentLookup;
            jobData.m_PropertyRenters = __TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentLookup;
            jobData.m_RenterBuffers = __TypeHandle.__Game_Buildings_Renter_RO_BufferLookup;
            jobData.m_Abandoneds = __TypeHandle.__Game_Buildings_Abandoned_RO_ComponentLookup;
            jobData.m_Destroyeds = __TypeHandle.__Game_Common_Destroyed_RO_ComponentLookup;
            jobData.m_ConsumptionDatas = __TypeHandle.__Game_Prefabs_ConsumptionData_RO_ComponentLookup;
            jobData.m_PropertyDatas = __TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup;
            jobData.m_Households = __TypeHandle.__Game_Citizens_Household_RO_ComponentLookup;
            jobData.m_Placeholders = __TypeHandle.__Game_Objects_Placeholder_RO_ComponentLookup;
            jobData.m_Attached = __TypeHandle.__Game_Objects_Attached_RO_ComponentLookup;
            jobData.m_SubAreas = __TypeHandle.__Game_Areas_SubArea_RO_BufferLookup;
            jobData.m_Lots = __TypeHandle.__Game_Areas_Lot_RO_ComponentLookup;
            jobData.m_Geometries = __TypeHandle.__Game_Areas_Geometry_RO_ComponentLookup;
            jobData.m_PollutionMap = m_GroundPollutionSystem.GetMap(readOnly: true, out var dependencies);
            jobData.m_PollutionParameters = m_PollutionParameterQuery.GetSingleton<PollutionParameterData>();
            jobHandle = jobData.ScheduleParallel(m_EdgeGroup, JobHandle.CombineDependencies(Dependency, dependencies));
            m_GroundPollutionSystem.AddReader(jobHandle);
        }
        if (!m_NodeGroup.IsEmptyIgnoreFilter)
        {
            __TypeHandle.__Game_Net_LandValue_RW_ComponentLookup.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Net_Curve_RO_ComponentLookup.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Net_ConnectedEdge_RO_BufferTypeHandle.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Net_Node_RO_ComponentTypeHandle.Update(ref CheckedStateRef);
            __TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref CheckedStateRef);
            NodeUpdateJob jobData2;
            jobData2 = default;
            jobData2.m_EntityType = __TypeHandle.__Unity_Entities_Entity_TypeHandle;
            jobData2.m_NodeType = __TypeHandle.__Game_Net_Node_RO_ComponentTypeHandle;
            jobData2.m_ConnectedEdgeType = __TypeHandle.__Game_Net_ConnectedEdge_RO_BufferTypeHandle;
            jobData2.m_Curves = __TypeHandle.__Game_Net_Curve_RO_ComponentLookup;
            jobData2.m_LandValues = __TypeHandle.__Game_Net_LandValue_RW_ComponentLookup;
            jobHandle = jobData2.ScheduleParallel(m_NodeGroup, jobHandle);
        }
        Dependency = jobHandle;
    }

    private static float GetDistanceFade(float distance)
    {
        return math.saturate(1f - distance / 720f);//vanilla=2000;��������>200�������ڵ����;
        //return 1f;0f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void __AssignQueries(ref SystemState state)
    {
    }

    protected override void OnCreateForCompiler()
    {
        base.OnCreateForCompiler();
        __AssignQueries(ref CheckedStateRef);
        __TypeHandle.__AssignHandles(ref CheckedStateRef);
    }

    [Preserve]
    public LandValueSystemMod()
    {
    }
}
