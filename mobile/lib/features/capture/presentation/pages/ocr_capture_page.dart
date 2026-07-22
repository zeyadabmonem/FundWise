import 'dart:io';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import 'package:image_picker/image_picker.dart';
import '../../../../core/themes/app_theme.dart';
import '../bloc/capture_bloc.dart';
import '../widgets/capture_confirmation_card.dart';

class OcrCapturePage extends StatefulWidget {
  const OcrCapturePage({super.key});

  @override
  State<OcrCapturePage> createState() => _OcrCapturePageState();
}

class _OcrCapturePageState extends State<OcrCapturePage> {
  final ImagePicker _picker = ImagePicker();
  File? _selectedImage;

  Future<void> _pickImage(ImageSource source) async {
    final picked = await _picker.pickImage(source: source, imageQuality: 80);
    if (picked != null) {
      setState(() => _selectedImage = File(picked.path));
      if (mounted) {
        context.read<CaptureBloc>().add(OcrImageSelected(File(picked.path)));
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Receipt OCR'),
        leading: IconButton(icon: const Icon(Icons.close), onPressed: () => context.pop()),
      ),
      body: BlocConsumer<CaptureBloc, CaptureState>(
        listener: (context, state) {
          if (state is CaptureSuccess) context.pop();
          if (state is CaptureError) {
            ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(state.message), backgroundColor: AppColors.error));
          }
        },
        builder: (context, state) {
          if (state is CapturePreview) return CaptureConfirmationCard(result: state.result);
          if (state is CaptureProcessing) {
            return Center(
              child: Column(mainAxisAlignment: MainAxisAlignment.center, children: [
                if (_selectedImage != null)
                  ClipRRect(
                    borderRadius: BorderRadius.circular(16),
                    child: Image.file(_selectedImage!, height: 200, fit: BoxFit.cover),
                  ),
                const SizedBox(height: 24),
                const CircularProgressIndicator(color: AppColors.primary),
                const SizedBox(height: 16),
                const Text('Extracting data from receipt…', style: TextStyle(color: AppColors.textSecondary)),
              ]),
            );
          }

          return Padding(
            padding: const EdgeInsets.all(24),
            child: Column(children: [
              // Preview or placeholder
              if (_selectedImage == null) ...[
                Container(
                  height: 240,
                  width: double.infinity,
                  decoration: BoxDecoration(
                    color: AppColors.surface,
                    borderRadius: BorderRadius.circular(20),
                    border: Border.all(color: AppColors.cardBorder, style: BorderStyle.solid),
                  ),
                  child: const Column(mainAxisAlignment: MainAxisAlignment.center, children: [
                    Icon(Icons.receipt_long_outlined, size: 56, color: AppColors.textMuted),
                    SizedBox(height: 12),
                    Text('No receipt selected', style: TextStyle(color: AppColors.textMuted, fontSize: 14)),
                  ]),
                ),
              ] else ...[
                ClipRRect(
                  borderRadius: BorderRadius.circular(20),
                  child: Image.file(_selectedImage!, height: 240, width: double.infinity, fit: BoxFit.cover),
                ),
              ],
              const SizedBox(height: 32),
              const Text('Scan a receipt', style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: AppColors.textPrimary)),
              const SizedBox(height: 8),
              const Text('Take a clear photo of the receipt or choose one from your gallery.', textAlign: TextAlign.center, style: TextStyle(color: AppColors.textSecondary, fontSize: 14)),
              const SizedBox(height: 32),
              Row(children: [
                Expanded(
                  child: _PickerButton(
                    icon: Icons.camera_alt_rounded,
                    label: 'Camera',
                    onTap: () => _pickImage(ImageSource.camera),
                  ),
                ),
                const SizedBox(width: 16),
                Expanded(
                  child: _PickerButton(
                    icon: Icons.photo_library_rounded,
                    label: 'Gallery',
                    onTap: () => _pickImage(ImageSource.gallery),
                  ),
                ),
              ]),
            ]),
          );
        },
      ),
    );
  }
}

class _PickerButton extends StatelessWidget {
  final IconData icon;
  final String label;
  final VoidCallback onTap;
  const _PickerButton({required this.icon, required this.label, required this.onTap});

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(vertical: 18),
        decoration: BoxDecoration(
          color: AppColors.surface,
          borderRadius: BorderRadius.circular(16),
          border: Border.all(color: AppColors.primary.withOpacity(0.4)),
        ),
        child: Column(children: [
          Icon(icon, color: AppColors.primary, size: 32),
          const SizedBox(height: 8),
          Text(label, style: const TextStyle(color: AppColors.primary, fontWeight: FontWeight.w600)),
        ]),
      ),
    );
  }
}
