import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import '../../../../core/themes/app_theme.dart';

class AddTransactionPage extends StatelessWidget {
  const AddTransactionPage({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Add Transaction'),
        leading: IconButton(
          icon: const Icon(Icons.close, color: AppColors.textPrimary),
          onPressed: () => context.pop(),
        ),
      ),
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Text(
                'How did you spend?',
                style: TextStyle(fontSize: 22, fontWeight: FontWeight.bold, color: AppColors.textPrimary),
              ),
              const SizedBox(height: 8),
              const Text(
                'Choose a capture method',
                style: TextStyle(fontSize: 14, color: AppColors.textSecondary),
              ),
              const SizedBox(height: 32),
              // Capture method grid
              Expanded(
                child: GridView.count(
                  crossAxisCount: 2,
                  crossAxisSpacing: 16,
                  mainAxisSpacing: 16,
                  childAspectRatio: 1.1,
                  children: [
                    _CaptureMethodCard(
                      icon: Icons.mic_rounded,
                      label: 'Voice',
                      subtitle: 'Speak your expense',
                      color: AppColors.primary,
                      onTap: () => context.push('/capture/voice'),
                    ),
                    _CaptureMethodCard(
                      icon: Icons.camera_alt_rounded,
                      label: 'Receipt OCR',
                      subtitle: 'Scan receipt photo',
                      color: AppColors.secondary,
                      onTap: () => context.push('/capture/ocr'),
                    ),
                    _CaptureMethodCard(
                      icon: Icons.qr_code_scanner_rounded,
                      label: 'QR Code',
                      subtitle: 'Scan digital receipt',
                      color: AppColors.transport,
                      onTap: () => context.push('/capture/qr'),
                    ),
                    _CaptureMethodCard(
                      icon: Icons.sms_rounded,
                      label: 'Bank SMS',
                      subtitle: 'Auto from SMS',
                      color: AppColors.bills,
                      onTap: () => context.push('/capture/sms'),
                    ),
                    _CaptureMethodCard(
                      icon: Icons.edit_rounded,
                      label: 'Manual Entry',
                      subtitle: 'Type it in',
                      color: AppColors.textSecondary,
                      onTap: () => context.push('/capture/manual'),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _CaptureMethodCard extends StatelessWidget {
  final IconData icon;
  final String label;
  final String subtitle;
  final Color color;
  final VoidCallback onTap;

  const _CaptureMethodCard({
    required this.icon,
    required this.label,
    required this.subtitle,
    required this.color,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 150),
        padding: const EdgeInsets.all(20),
        decoration: BoxDecoration(
          color: AppColors.surface,
          borderRadius: BorderRadius.circular(20),
          border: Border.all(color: AppColors.cardBorder),
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Container(
              width: 44,
              height: 44,
              decoration: BoxDecoration(
                color: color.withOpacity(0.15),
                borderRadius: BorderRadius.circular(12),
              ),
              child: Icon(icon, color: color, size: 24),
            ),
            Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(label, style: const TextStyle(fontSize: 15, fontWeight: FontWeight.bold, color: AppColors.textPrimary)),
                const SizedBox(height: 2),
                Text(subtitle, style: const TextStyle(fontSize: 11, color: AppColors.textSecondary)),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
