import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import 'package:record/record.dart';
import '../../../../core/themes/app_theme.dart';
import '../bloc/capture_bloc.dart';
import '../widgets/capture_confirmation_card.dart';

class VoiceCapturePage extends StatefulWidget {
  const VoiceCapturePage({super.key});

  @override
  State<VoiceCapturePage> createState() => _VoiceCapturePageState();
}

class _VoiceCapturePageState extends State<VoiceCapturePage> with SingleTickerProviderStateMixin {
  final AudioRecorder _recorder = AudioRecorder();
  bool _isRecording = false;
  late AnimationController _pulseController;
  late Animation<double> _pulseAnim;

  @override
  void initState() {
    super.initState();
    _pulseController = AnimationController(vsync: this, duration: const Duration(milliseconds: 900))..repeat(reverse: true);
    _pulseAnim = Tween<double>(begin: 1.0, end: 1.25).animate(CurvedAnimation(parent: _pulseController, curve: Curves.easeInOut));
  }

  @override
  void dispose() {
    _pulseController.dispose();
    _recorder.dispose();
    super.dispose();
  }

  Future<void> _startRecording() async {
    if (await _recorder.hasPermission()) {
      final dir = await getTemporaryDirectory();
      final path = '${dir.path}/voice_capture.m4a';
      await _recorder.start(const RecordConfig(), path: path);
      setState(() => _isRecording = true);
    }
  }

  Future<void> _stopRecording() async {
    final path = await _recorder.stop();
    setState(() => _isRecording = false);
    if (path != null && mounted) {
      context.read<CaptureBloc>().add(VoiceRecordingStopped(path));
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Voice Capture'),
        leading: IconButton(icon: const Icon(Icons.close), onPressed: () => context.pop()),
      ),
      body: BlocConsumer<CaptureBloc, CaptureState>(
        listener: (context, state) {
          if (state is CaptureSuccess) context.pop();
          if (state is CaptureError) {
            ScaffoldMessenger.of(context).showSnackBar(SnackBar(
              content: Text(state.message),
              backgroundColor: AppColors.error,
            ));
          }
        },
        builder: (context, state) {
          if (state is CapturePreview) {
            return CaptureConfirmationCard(result: state.result);
          }
          if (state is CaptureProcessing) {
            return const Center(
              child: Column(mainAxisAlignment: MainAxisAlignment.center, children: [
                CircularProgressIndicator(color: AppColors.primary),
                SizedBox(height: 16),
                Text('Processing audio…', style: TextStyle(color: AppColors.textSecondary)),
              ]),
            );
          }

          return Center(
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                const Text(
                  'Tap and hold to record\nyour expense in your own words',
                  textAlign: TextAlign.center,
                  style: TextStyle(color: AppColors.textSecondary, fontSize: 15, height: 1.6),
                ),
                const SizedBox(height: 56),

                // Pulse ring + mic button
                AnimatedBuilder(
                  animation: _pulseAnim,
                  builder: (context, child) {
                    return Stack(alignment: Alignment.center, children: [
                      if (_isRecording)
                        Transform.scale(
                          scale: _pulseAnim.value,
                          child: Container(
                            width: 120,
                            height: 120,
                            decoration: BoxDecoration(
                              color: AppColors.error.withOpacity(0.15),
                              shape: BoxShape.circle,
                            ),
                          ),
                        ),
                      GestureDetector(
                        onTapDown: (_) => _startRecording(),
                        onTapUp: (_) => _stopRecording(),
                        child: Container(
                          width: 96,
                          height: 96,
                          decoration: BoxDecoration(
                            gradient: LinearGradient(
                              colors: _isRecording
                                  ? [AppColors.error, const Color(0xFFFF8A80)]
                                  : [AppColors.primary, AppColors.primaryLight],
                            ),
                            shape: BoxShape.circle,
                            boxShadow: [
                              BoxShadow(
                                color: (_isRecording ? AppColors.error : AppColors.primary).withOpacity(0.4),
                                blurRadius: 24,
                                spreadRadius: 4,
                              ),
                            ],
                          ),
                          child: Icon(
                            _isRecording ? Icons.stop_rounded : Icons.mic_rounded,
                            color: Colors.white,
                            size: 44,
                          ),
                        ),
                      ),
                    ]);
                  },
                ),

                const SizedBox(height: 32),
                AnimatedOpacity(
                  opacity: _isRecording ? 1.0 : 0.0,
                  duration: const Duration(milliseconds: 300),
                  child: const Text('Recording… tap to stop', style: TextStyle(color: AppColors.error, fontSize: 14, fontWeight: FontWeight.w500)),
                ),

                const SizedBox(height: 48),
                const Padding(
                  padding: EdgeInsets.symmetric(horizontal: 48),
                  child: Text(
                    'Example: "اشتريت قهوة من ستارباكس بـ ١٢٠ جنيه النهارده"',
                    textAlign: TextAlign.center,
                    style: TextStyle(color: AppColors.textMuted, fontSize: 12, fontStyle: FontStyle.italic),
                  ),
                ),
              ],
            ),
          );
        },
      ),
    );
  }
}
