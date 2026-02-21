// CRTRenderFeature.cs

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CRTRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        public Material crtMaterial;
    }

    public Settings settings = new Settings();
    private CRTRenderPass renderPass;

    public override void Create()
    {
        renderPass = new CRTRenderPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.crtMaterial == null) return;
        renderer.EnqueuePass(renderPass);
    }

    private class CRTRenderPass : ScriptableRenderPass
    {
        private Material material;
        private RTHandle tempRT;

        public CRTRenderPass(Settings settings)
        {
            this.material = settings.crtMaterial;
            this.renderPassEvent = settings.renderPassEvent;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            RenderingUtils.ReAllocateIfNeeded(ref tempRT, descriptor, name: "_CRTTempRT");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null) return;

            CommandBuffer cmd = CommandBufferPool.Get("CRT Scanlines");

            RTHandle source = renderingData.cameraData.renderer.cameraColorTargetHandle;

            Blit(cmd, source, tempRT, material, 0);
            Blit(cmd, tempRT, source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            // RTHandle se gestiona por ReAllocateIfNeeded
        }
    }
}