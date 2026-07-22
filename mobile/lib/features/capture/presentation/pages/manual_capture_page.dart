import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import '../../../../core/themes/app_theme.dart';
import '../bloc/capture_bloc.dart';
import '../widgets/capture_confirmation_card.dart';

class ManualCapturePage extends StatefulWidget {
  const ManualCapturePage({super.key});

  @override
  State<ManualCapturePage> createState() => _ManualCapturePageState();
}

class _ManualCapturePageState extends State<ManualCapturePage> {
  final _formKey = GlobalKey<FormState>();
  final _merchantController = TextEditingController();
  final _amountController = TextEditingController();
  final _notesController = TextEditingController();
  String _selectedCategory = 'Other';
  DateTime _selectedDate = DateTime.now();

  final List<Map<String, dynamic>> _categories = [
    {'key': 'FoodAndDrink', 'label': 'Food & Drink', 'icon': Icons.restaurant_rounded},
    {'key': 'Groceries', 'label': 'Groceries', 'icon': Icons.shopping_cart_rounded},
    {'key': 'Transport', 'label': 'Transport', 'icon': Icons.directions_car_rounded},
    {'key': 'BillsAndUtilities', 'label': 'Bills', 'icon': Icons.receipt_rounded},
    {'key': 'Shopping', 'label': 'Shopping', 'icon': Icons.shopping_bag_rounded},
    {'key': 'Entertainment', 'label': 'Entertainment', 'icon': Icons.movie_rounded},
    {'key': 'Health', 'label': 'Health', 'icon': Icons.medical_services_rounded},
    {'key': 'Education', 'label': 'Education', 'icon': Icons.school_rounded},
    {'key': 'Other', 'label': 'Other', 'icon': Icons.category_rounded},
  ];

  @override
  void dispose() {
    _merchantController.dispose();
    _amountController.dispose();
    _notesController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Manual Entry'),
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

          return SingleChildScrollView(
            padding: const EdgeInsets.all(24),
            child: Form(
              key: _formKey,
              child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                // Merchant
                const Text('Merchant / Description', style: TextStyle(fontSize: 14, fontWeight: FontWeight.w600, color: AppColors.textPrimary)),
                const SizedBox(height: 8),
                TextFormField(
                  controller: _merchantController,
                  style: const TextStyle(color: AppColors.textPrimary),
                  decoration: _inputDec('e.g. Starbucks Cairo Festival City', Icons.store_outlined),
                  validator: (v) => (v == null || v.isEmpty) ? 'Required' : null,
                ),
                const SizedBox(height: 20),

                // Amount
                const Text('Amount (EGP)', style: TextStyle(fontSize: 14, fontWeight: FontWeight.w600, color: AppColors.textPrimary)),
                const SizedBox(height: 8),
                TextFormField(
                  controller: _amountController,
                  keyboardType: const TextInputType.numberWithOptions(decimal: true),
                  style: const TextStyle(color: AppColors.textPrimary),
                  decoration: _inputDec('0.00', Icons.attach_money_rounded),
                  validator: (v) {
                    if (v == null || v.isEmpty) return 'Required';
                    if (double.tryParse(v) == null) return 'Enter a valid number';
                    return null;
                  },
                ),
                const SizedBox(height: 20),

                // Category
                const Text('Category', style: TextStyle(fontSize: 14, fontWeight: FontWeight.w600, color: AppColors.textPrimary)),
                const SizedBox(height: 12),
                Wrap(
                  spacing: 8,
                  runSpacing: 8,
                  children: _categories.map((cat) {
                    final isSelected = _selectedCategory == cat['key'];
                    return GestureDetector(
                      onTap: () => setState(() => _selectedCategory = cat['key']),
                      child: AnimatedContainer(
                        duration: const Duration(milliseconds: 150),
                        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
                        decoration: BoxDecoration(
                          color: isSelected ? AppColors.primary.withOpacity(0.15) : AppColors.surface,
                          borderRadius: BorderRadius.circular(20),
                          border: Border.all(color: isSelected ? AppColors.primary : AppColors.cardBorder),
                        ),
                        child: Row(mainAxisSize: MainAxisSize.min, children: [
                          Icon(cat['icon'] as IconData, size: 14, color: isSelected ? AppColors.primary : AppColors.textSecondary),
                          const SizedBox(width: 6),
                          Text(cat['label'] as String, style: TextStyle(fontSize: 12, color: isSelected ? AppColors.primary : AppColors.textSecondary, fontWeight: isSelected ? FontWeight.bold : FontWeight.normal)),
                        ]),
                      ),
                    );
                  }).toList(),
                ),
                const SizedBox(height: 20),

                // Date
                const Text('Date', style: TextStyle(fontSize: 14, fontWeight: FontWeight.w600, color: AppColors.textPrimary)),
                const SizedBox(height: 8),
                GestureDetector(
                  onTap: _pickDate,
                  child: Container(
                    padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
                    decoration: BoxDecoration(
                      color: AppColors.surface,
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(color: AppColors.cardBorder),
                    ),
                    child: Row(children: [
                      const Icon(Icons.calendar_today_rounded, size: 18, color: AppColors.textSecondary),
                      const SizedBox(width: 10),
                      Text(
                        '${_selectedDate.day}/${_selectedDate.month}/${_selectedDate.year}',
                        style: const TextStyle(color: AppColors.textPrimary, fontSize: 15),
                      ),
                    ]),
                  ),
                ),
                const SizedBox(height: 20),

                // Notes
                const Text('Notes (optional)', style: TextStyle(fontSize: 14, fontWeight: FontWeight.w600, color: AppColors.textPrimary)),
                const SizedBox(height: 8),
                TextFormField(
                  controller: _notesController,
                  style: const TextStyle(color: AppColors.textPrimary),
                  maxLines: 2,
                  decoration: _inputDec('Any notes…', Icons.notes_rounded),
                ),
                const SizedBox(height: 32),

                SizedBox(
                  width: double.infinity,
                  child: ElevatedButton(
                    onPressed: _onSubmit,
                    child: const Text('Preview & Confirm'),
                  ),
                ),
              ]),
            ),
          );
        },
      ),
    );
  }

  InputDecoration _inputDec(String hint, IconData icon) => InputDecoration(
        hintText: hint,
        hintStyle: const TextStyle(color: AppColors.textMuted),
        prefixIcon: Icon(icon, color: AppColors.textSecondary, size: 20),
        filled: true,
        fillColor: AppColors.surface,
        enabledBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: const BorderSide(color: AppColors.cardBorder)),
        focusedBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: const BorderSide(color: AppColors.primary, width: 1.5)),
        errorBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: const BorderSide(color: AppColors.error)),
        focusedErrorBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: const BorderSide(color: AppColors.error, width: 1.5)),
        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
      );

  Future<void> _pickDate() async {
    final picked = await showDatePicker(
      context: context,
      initialDate: _selectedDate,
      firstDate: DateTime(2020),
      lastDate: DateTime.now(),
      builder: (context, child) => Theme(
        data: ThemeData.dark().copyWith(
          colorScheme: const ColorScheme.dark(primary: AppColors.primary, surface: AppColors.surface),
        ),
        child: child!,
      ),
    );
    if (picked != null) setState(() => _selectedDate = picked);
  }

  void _onSubmit() {
    if (_formKey.currentState?.validate() ?? false) {
      context.read<CaptureBloc>().add(ManualEntrySubmitted(
        merchant: _merchantController.text.trim(),
        amount: double.parse(_amountController.text),
        category: _selectedCategory,
        notes: _notesController.text.isEmpty ? null : _notesController.text.trim(),
      ));
    }
  }
}
