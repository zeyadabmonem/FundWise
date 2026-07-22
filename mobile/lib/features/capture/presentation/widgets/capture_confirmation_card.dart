import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import '../../../../core/themes/app_theme.dart';
import '../bloc/capture_bloc.dart';

class CaptureConfirmationCard extends StatelessWidget {
  final CaptureResult result;
  const CaptureConfirmationCard({super.key, required this.result});

  @override
  Widget build(BuildContext context) {
    return SafeArea(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text('Review & Confirm', style: TextStyle(fontSize: 22, fontWeight: FontWeight.bold, color: AppColors.textPrimary)),
            const SizedBox(height: 6),
            const Text('Please review the detected expense', style: TextStyle(color: AppColors.textSecondary, fontSize: 14)),
            const SizedBox(height: 24),

            // Confidence indicator
            _ConfidenceBadge(score: result.confidenceScore),
            const SizedBox(height: 20),

            // Main card
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(20),
              decoration: BoxDecoration(
                color: AppColors.surface,
                borderRadius: BorderRadius.circular(20),
                border: Border.all(color: AppColors.cardBorder),
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  _Row('Merchant', result.merchant),
                  const Divider(color: AppColors.cardBorder, height: 24),
                  _Row('Amount', 'EGP ${result.amount.toStringAsFixed(2)}', valueColor: AppColors.primary),
                  const Divider(color: AppColors.cardBorder, height: 24),
                  _Row('Category', result.categoryName),
                  const Divider(color: AppColors.cardBorder, height: 24),
                  _Row('Source', result.captureSource),
                ],
              ),
            ),

            const Spacer(),

            // Action buttons
            Row(children: [
              Expanded(
                child: OutlinedButton(
                  onPressed: () => context.read<CaptureBloc>().add(CaptureCancelled()),
                  style: OutlinedButton.styleFrom(
                    side: const BorderSide(color: AppColors.cardBorder),
                    foregroundColor: AppColors.textSecondary,
                    padding: const EdgeInsets.symmetric(vertical: 14),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
                  ),
                  child: const Text('Retake'),
                ),
              ),
              const SizedBox(width: 16),
              Expanded(
                flex: 2,
                child: ElevatedButton(
                  onPressed: () => context.read<CaptureBloc>().add(CaptureConfirmed()),
                  style: ElevatedButton.styleFrom(
                    backgroundColor: AppColors.primary,
                    padding: const EdgeInsets.symmetric(vertical: 14),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
                  ),
                  child: const Row(mainAxisAlignment: MainAxisAlignment.center, children: [
                    Icon(Icons.check_circle_rounded, color: Colors.white, size: 18),
                    SizedBox(width: 8),
                    Text('Confirm & Save', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
                  ]),
                ),
              ),
            ]),
          ],
        ),
      ),
    );
  }
}

class _Row extends StatelessWidget {
  final String label;
  final String value;
  final Color? valueColor;
  const _Row(this.label, this.value, {this.valueColor});

  @override
  Widget build(BuildContext context) => Row(
    mainAxisAlignment: MainAxisAlignment.spaceBetween,
    children: [
      Text(label, style: const TextStyle(color: AppColors.textSecondary, fontSize: 14)),
      Text(value, style: TextStyle(color: valueColor ?? AppColors.textPrimary, fontWeight: FontWeight.w600, fontSize: 14)),
    ],
  );
}

class _ConfidenceBadge extends StatelessWidget {
  final double score;
  const _ConfidenceBadge({required this.score});

  @override
  Widget build(BuildContext context) {
    final color = score >= 0.85 ? AppColors.success : score >= 0.6 ? AppColors.warning : AppColors.error;
    final label = score >= 0.85 ? 'High Confidence' : score >= 0.6 ? 'Moderate Confidence' : 'Low Confidence';

    return Row(children: [
      Container(
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
        decoration: BoxDecoration(color: color.withOpacity(0.15), borderRadius: BorderRadius.circular(20)),
        child: Row(children: [
          Icon(Icons.auto_awesome_rounded, size: 14, color: color),
          const SizedBox(width: 6),
          Text(label, style: TextStyle(color: color, fontSize: 12, fontWeight: FontWeight.bold)),
          const SizedBox(width: 6),
          Text('${(score * 100).toStringAsFixed(0)}%', style: TextStyle(color: color, fontSize: 12)),
        ]),
      ),
    ]);
  }
}
