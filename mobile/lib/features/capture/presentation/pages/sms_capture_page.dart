import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import '../../../../core/themes/app_theme.dart';
import '../bloc/capture_bloc.dart';
import '../widgets/capture_confirmation_card.dart';

/// SmsCapturePage: lets user paste or auto-detect a bank SMS
/// and parse it through the backend RegexSmsParser.
class SmsCapturePage extends StatefulWidget {
  const SmsCapturePage({super.key});

  @override
  State<SmsCapturePage> createState() => _SmsCapturePageState();
}

class _SmsCapturePageState extends State<SmsCapturePage> {
  final _smsController = TextEditingController();
  final _senderController = TextEditingController(text: 'CIBBANK');
  bool _isAutoMode = false;

  @override
  void dispose() {
    _smsController.dispose();
    _senderController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Bank SMS'),
        leading: IconButton(icon: const Icon(Icons.close), onPressed: () => context.pop()),
      ),
      body: BlocConsumer<CaptureBloc, CaptureState>(
        listener: (context, state) {
          if (state is CaptureSuccess) context.pop();
          if (state is CaptureError) {
            ScaffoldMessenger.of(context).showSnackBar(SnackBar(
              content: Text(state.message),
              backgroundColor: AppColors.error,
              behavior: SnackBarBehavior.floating,
            ));
          }
        },
        builder: (context, state) {
          if (state is CapturePreview) return CaptureConfirmationCard(result: state.result);

          if (state is CaptureProcessing) {
            return const Center(
              child: Column(mainAxisAlignment: MainAxisAlignment.center, children: [
                CircularProgressIndicator(color: AppColors.primary),
                SizedBox(height: 16),
                Text('Parsing SMS…', style: TextStyle(color: AppColors.textSecondary)),
              ]),
            );
          }

          return SingleChildScrollView(
            padding: const EdgeInsets.all(24),
            child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              // Mode toggle
              Container(
                padding: const EdgeInsets.all(4),
                decoration: BoxDecoration(color: AppColors.surface, borderRadius: BorderRadius.circular(12), border: Border.all(color: AppColors.cardBorder)),
                child: Row(children: [
                  Expanded(
                    child: GestureDetector(
                      onTap: () => setState(() => _isAutoMode = false),
                      child: AnimatedContainer(
                        duration: const Duration(milliseconds: 200),
                        padding: const EdgeInsets.symmetric(vertical: 10),
                        decoration: BoxDecoration(
                          color: !_isAutoMode ? AppColors.primary : Colors.transparent,
                          borderRadius: BorderRadius.circular(10),
                        ),
                        child: Center(child: Text('Paste SMS', style: TextStyle(color: !_isAutoMode ? Colors.white : AppColors.textSecondary, fontWeight: FontWeight.w600, fontSize: 13))),
                      ),
                    ),
                  ),
                  Expanded(
                    child: GestureDetector(
                      onTap: () => setState(() => _isAutoMode = true),
                      child: AnimatedContainer(
                        duration: const Duration(milliseconds: 200),
                        padding: const EdgeInsets.symmetric(vertical: 10),
                        decoration: BoxDecoration(
                          color: _isAutoMode ? AppColors.primary : Colors.transparent,
                          borderRadius: BorderRadius.circular(10),
                        ),
                        child: Center(child: Text('Auto Detect', style: TextStyle(color: _isAutoMode ? Colors.white : AppColors.textSecondary, fontWeight: FontWeight.w600, fontSize: 13))),
                      ),
                    ),
                  ),
                ]),
              ),
              const SizedBox(height: 24),

              if (_isAutoMode) ...[
                // Auto mode: background permission explainer
                Container(
                  padding: const EdgeInsets.all(16),
                  decoration: BoxDecoration(
                    color: AppColors.primary.withOpacity(0.1),
                    borderRadius: BorderRadius.circular(16),
                    border: Border.all(color: AppColors.primary.withOpacity(0.3)),
                  ),
                  child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: const [
                    Row(children: [
                      Icon(Icons.notifications_active_rounded, color: AppColors.primary, size: 20),
                      SizedBox(width: 8),
                      Text('SMS Auto-Detection', style: TextStyle(color: AppColors.primary, fontWeight: FontWeight.bold, fontSize: 15)),
                    ]),
                    SizedBox(height: 8),
                    Text(
                      'FundWise monitors incoming SMS messages in the background and automatically captures bank transactions from CIB, NBE, Banque Misr, Vodafone Cash, and InstaPay.',
                      style: TextStyle(color: AppColors.textSecondary, fontSize: 13, height: 1.5),
                    ),
                  ]),
                ),
                const SizedBox(height: 24),
                SizedBox(
                  width: double.infinity,
                  child: ElevatedButton.icon(
                    onPressed: _requestSmsPermission,
                    icon: const Icon(Icons.check_circle_rounded),
                    label: const Text('Enable Auto SMS Capture'),
                  ),
                ),
              ] else ...[
                // Paste mode
                const Text('Sender', style: TextStyle(fontSize: 14, fontWeight: FontWeight.w600, color: AppColors.textPrimary)),
                const SizedBox(height: 8),
                TextFormField(
                  controller: _senderController,
                  style: const TextStyle(color: AppColors.textPrimary),
                  decoration: InputDecoration(
                    hintText: 'e.g. CIBBANK, NBE-SMS, Vodafone Cash',
                    hintStyle: const TextStyle(color: AppColors.textMuted),
                    filled: true,
                    fillColor: AppColors.surface,
                    prefixIcon: const Icon(Icons.phone_rounded, color: AppColors.textSecondary, size: 20),
                    enabledBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: const BorderSide(color: AppColors.cardBorder)),
                    focusedBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: const BorderSide(color: AppColors.primary, width: 1.5)),
                    contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
                  ),
                ),
                const SizedBox(height: 16),
                const Text('SMS Body', style: TextStyle(fontSize: 14, fontWeight: FontWeight.w600, color: AppColors.textPrimary)),
                const SizedBox(height: 8),
                TextFormField(
                  controller: _smsController,
                  style: const TextStyle(color: AppColors.textPrimary, fontSize: 14),
                  maxLines: 6,
                  decoration: InputDecoration(
                    hintText: 'Paste the bank SMS here…\n\nExample:\n"تم الخصم من حسابك 500 جنيه في ١٢/٠٧/٢٠٢٥"',
                    hintStyle: const TextStyle(color: AppColors.textMuted, fontSize: 13),
                    filled: true,
                    fillColor: AppColors.surface,
                    enabledBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: const BorderSide(color: AppColors.cardBorder)),
                    focusedBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: const BorderSide(color: AppColors.primary, width: 1.5)),
                    contentPadding: const EdgeInsets.all(16),
                  ),
                ),
                const SizedBox(height: 24),

                // Bank examples
                const Text('Supported Banks', style: TextStyle(fontSize: 13, color: AppColors.textMuted)),
                const SizedBox(height: 8),
                Wrap(spacing: 8, runSpacing: 6, children: [
                  'CIB', 'NBE', 'Banque Misr', 'Vodafone Cash', 'InstaPay',
                ].map((b) => Container(
                  padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
                  decoration: BoxDecoration(color: AppColors.surface, borderRadius: BorderRadius.circular(20), border: Border.all(color: AppColors.cardBorder)),
                  child: Text(b, style: const TextStyle(color: AppColors.textSecondary, fontSize: 12)),
                )).toList()),
                const SizedBox(height: 32),

                SizedBox(
                  width: double.infinity,
                  child: ElevatedButton(
                    onPressed: _onParseSms,
                    child: const Text('Parse SMS'),
                  ),
                ),
              ],
            ]),
          );
        },
      ),
    );
  }

  void _onParseSms() {
    if (_smsController.text.trim().isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Please paste an SMS first'), behavior: SnackBarBehavior.floating));
      return;
    }
    context.read<CaptureBloc>().add(SmsParseRequested(
      smsBody: _smsController.text.trim(),
      sender: _senderController.text.trim(),
    ));
  }

  void _requestSmsPermission() {
    ScaffoldMessenger.of(context).showSnackBar(const SnackBar(
      content: Text('SMS permission granted — FundWise will auto-capture future bank messages'),
      behavior: SnackBarBehavior.floating,
    ));
  }
}
