using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SSPRBlurRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Setting
    {
        public RenderPassEvent PassEvent=RenderPassEvent.AfterRenderingOpaques;
        public Material blurMat;
        [Range(1,15)]public int Loop=1;
        public float BlurSize=1;
        [Range(1,4)]public int BlurDownSample = 1;
    }
    public Setting setting;


    class CustomRenderPass : ScriptableRenderPass
    {
        static readonly string k_RenderTag = "SSPRBlurPass";

        int temp01 = Shader.PropertyToID("_ReflectBlurTemp01");
        int temp02 = Shader.PropertyToID("_ReflectBlurTemp02");


        Setting m_setting;
        Material blurMaterial;
        RenderTargetIdentifier cameraTarget;

        public void Setup(Setting setting,in RenderTargetIdentifier cameraTarget)
        {
            m_setting = setting;
            this.cameraTarget=cameraTarget; 
        }
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var cameraDesc = renderingData.cameraData.cameraTargetDescriptor;
            float aspect=(float)cameraDesc.width/(float)cameraDesc.height;
            int height=512;
            int width=Mathf.RoundToInt(512 * aspect);
            height /= m_setting.BlurDownSample;
            width /= m_setting.BlurDownSample;  
            var desc = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGB32);
            cmd.GetTemporaryRT(temp01,desc);
            cmd.GetTemporaryRT(temp02,desc);

        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (m_setting.blurMat == null) return;
            blurMaterial = m_setting.blurMat;

            //if (renderingData.cameraData.isSceneViewCamera) return;//不处理scene的相机
            var cmd = CommandBufferPool.Get(k_RenderTag);
            Render(cmd,ref renderingData);
            context.ExecuteCommandBuffer(cmd);
           
            CommandBufferPool.Release(cmd);
        }

        void Render(CommandBuffer cmd,ref RenderingData renderingData)
        {

            cmd.Blit(new RenderTargetIdentifier("_ReflectRT"), temp01);
            int iteration = m_setting.Loop;
            float size = 0;
            for (int i = 0; i < iteration; i++)
            {
                size = (i + 1) * m_setting.BlurSize;
                blurMaterial.SetFloat("_Size", size);
                cmd.Blit(temp01, temp02, blurMaterial, 0);
                cmd.Blit(temp02, temp01, blurMaterial, 0);


            }
            cmd.Blit(temp01, new RenderTargetIdentifier("_ReflectRT"));
            cmd.SetRenderTarget(cameraTarget);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(temp01);
            cmd.ReleaseTemporaryRT(temp02);
        }
    }

    CustomRenderPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass();
        m_ScriptablePass.renderPassEvent = setting.PassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass.Setup(setting,renderer.cameraColorTarget);
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


