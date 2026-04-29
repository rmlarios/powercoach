'use client';

import { useState } from 'react';
import { PageHeader } from '@/components/common';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Plus, DollarSign, TrendingUp, Clock, AlertCircle } from 'lucide-react';
import { usePayments, useRegisterPayment } from '@/hooks';
import { useAthletes } from '@/hooks';
import { useCoach, useAuth } from '@/providers';
import { PaymentStatus } from '@/types';
import { format } from 'date-fns';
import { toast } from 'sonner';

const STATUS_COLORS: Record<PaymentStatus, string> = {
  Completed: 'bg-green-100 text-green-800',
  Pending: 'bg-yellow-100 text-yellow-800',
  Failed: 'bg-red-100 text-red-800',
  Refunded: 'bg-blue-100 text-blue-800',
  Cancelled: 'bg-slate-100 text-slate-800',
};

export default function PaymentsPage() {
  const { user } = useAuth();
  const { coach } = useCoach();
  const coachId = user?.coachId ?? coach?.id ?? '';
  const [showRegister, setShowRegister] = useState(false);
  const [filterAthleteId, setFilterAthleteId] = useState('');
  const [filterMethod, setFilterMethod] = useState('');

  // Register form state
  const [athleteId, setAthleteId] = useState('');
  const [amount, setAmount] = useState('');
  const [currency, setCurrency] = useState('USD');
  const [paymentMethod, setPaymentMethod] = useState('');
  const [paymentDate, setPaymentDate] = useState(new Date().toISOString().slice(0, 10));
  const [notes, setNotes] = useState('');

  const { data: athletes } = useAthletes({ coachId });
  const { data: payments, isLoading } = usePayments({
    athleteId: filterAthleteId || undefined,
    paymentMethod: filterMethod || undefined,
  });
  const registerPayment = useRegisterPayment();

  const handleRegister = () => {
    if (!athleteId || !amount || !paymentDate) return;

    registerPayment.mutate(
      {
        athleteId,
        subscriptionId: '',
        amount: parseFloat(amount),
        currency,
        paymentMethod: paymentMethod || undefined,
        paymentDate,
        notes: notes || undefined,
      },
      {
        onSuccess: () => {
          toast.success('Payment registered');
          setShowRegister(false);
          setAthleteId('');
          setAmount('');
          setPaymentMethod('');
          setNotes('');
        },
        onError: () => toast.error('Failed to register payment'),
      }
    );
  };

  // Summary calculations
  const totalReceived = payments?.filter((p) => p.status === 'Completed').reduce((sum, p) => sum + p.amount, 0) ?? 0;
  const totalPending = payments?.filter((p) => p.status === 'Pending').reduce((sum, p) => sum + p.amount, 0) ?? 0;
  const completedCount = payments?.filter((p) => p.status === 'Completed').length ?? 0;
  const pendingCount = payments?.filter((p) => p.status === 'Pending').length ?? 0;

  return (
    <div>
      <PageHeader
        title="Payments"
        description="Track and register athlete payments"
        actions={
          <Button onClick={() => setShowRegister(true)}>
            <Plus className="w-4 h-4 mr-2" />
            Register Payment
          </Button>
        }
      />

      {/* Summary Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-green-100 rounded-lg">
                <DollarSign className="h-5 w-5 text-green-600" />
              </div>
              <div>
                <p className="text-sm text-slate-500">Total Received</p>
                <p className="text-xl font-bold text-slate-900">${totalReceived.toFixed(2)}</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-yellow-100 rounded-lg">
                <Clock className="h-5 w-5 text-yellow-600" />
              </div>
              <div>
                <p className="text-sm text-slate-500">Pending</p>
                <p className="text-xl font-bold text-slate-900">${totalPending.toFixed(2)}</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-green-100 rounded-lg">
                <TrendingUp className="h-5 w-5 text-green-600" />
              </div>
              <div>
                <p className="text-sm text-slate-500">Completed</p>
                <p className="text-xl font-bold text-slate-900">{completedCount}</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-orange-100 rounded-lg">
                <AlertCircle className="h-5 w-5 text-orange-600" />
              </div>
              <div>
                <p className="text-sm text-slate-500">Pending</p>
                <p className="text-xl font-bold text-slate-900">{pendingCount}</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Filters */}
      <div className="flex flex-col sm:flex-row gap-4 mb-6">
        <Select value={filterAthleteId} onValueChange={setFilterAthleteId}>
          <SelectTrigger className="max-w-xs">
            <SelectValue placeholder="All athletes" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Athletes</SelectItem>
            {athletes?.items?.map((a) => (
              <SelectItem key={a.id} value={a.id}>{a.fullName}</SelectItem>
            ))}
          </SelectContent>
        </Select>
        <Select value={filterMethod} onValueChange={setFilterMethod}>
          <SelectTrigger className="max-w-xs">
            <SelectValue placeholder="All methods" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Methods</SelectItem>
            <SelectItem value="Cash">Cash</SelectItem>
            <SelectItem value="Transfer">Transfer</SelectItem>
            <SelectItem value="Card">Card</SelectItem>
            <SelectItem value="PayPal">PayPal</SelectItem>
            <SelectItem value="Zelle">Zelle</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {/* Payments Table */}
      {isLoading ? (
        <Card>
          <CardContent className="space-y-3 py-6">
            {[1, 2, 3].map((i) => <Skeleton key={i} className="h-12 w-full" />)}
          </CardContent>
        </Card>
      ) : payments && payments.length > 0 ? (
        <Card>
          <CardContent className="p-0">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Athlete</TableHead>
                  <TableHead>Amount</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Method</TableHead>
                  <TableHead>Date</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {payments.map((payment) => (
                  <TableRow key={payment.id}>
                    <TableCell className="font-medium">{payment.athleteName}</TableCell>
                    <TableCell>
                      <span className="font-semibold">${payment.amount.toFixed(2)}</span>
                      <span className="text-slate-400 ml-1 text-xs">{payment.currency}</span>
                    </TableCell>
                    <TableCell>
                      <Badge className={STATUS_COLORS[payment.status]}>{payment.status}</Badge>
                    </TableCell>
                    <TableCell className="text-sm text-slate-600">
                      {payment.paymentMethod || '—'}
                    </TableCell>
                    <TableCell className="text-sm text-slate-600">
                      {format(new Date(payment.paymentDate), 'MMM d, yyyy')}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      ) : (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <DollarSign className="h-12 w-12 text-slate-300 mb-4" />
            <h3 className="text-lg font-medium text-slate-900 mb-1">No payments recorded</h3>
            <p className="text-slate-500 mb-4">Register your first payment to start tracking</p>
            <Button onClick={() => setShowRegister(true)}>
              <Plus className="w-4 h-4 mr-2" />
              Register Payment
            </Button>
          </CardContent>
        </Card>
      )}

      {/* Register Payment Dialog */}
      <Dialog open={showRegister} onOpenChange={setShowRegister}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Register Payment</DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label>Athlete *</Label>
              <Select value={athleteId} onValueChange={setAthleteId}>
                <SelectTrigger>
                  <SelectValue placeholder="Select athlete..." />
                </SelectTrigger>
                <SelectContent>
                  {athletes?.items?.map((a) => (
                    <SelectItem key={a.id} value={a.id}>{a.fullName}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="pay-amount">Amount *</Label>
                <Input
                  id="pay-amount"
                  type="number"
                  min="0"
                  step="0.01"
                  placeholder="0.00"
                  value={amount}
                  onChange={(e) => setAmount(e.target.value)}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="pay-currency">Currency</Label>
                <Input
                  id="pay-currency"
                  value={currency}
                  onChange={(e) => setCurrency(e.target.value)}
                />
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Payment Method</Label>
                <Select value={paymentMethod} onValueChange={setPaymentMethod}>
                  <SelectTrigger>
                    <SelectValue placeholder="Select method..." />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Cash">Cash</SelectItem>
                    <SelectItem value="Transfer">Bank Transfer</SelectItem>
                    <SelectItem value="Card">Card</SelectItem>
                    <SelectItem value="PayPal">PayPal</SelectItem>
                    <SelectItem value="Zelle">Zelle</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="pay-date">Payment Date *</Label>
                <Input
                  id="pay-date"
                  type="date"
                  value={paymentDate}
                  onChange={(e) => setPaymentDate(e.target.value)}
                />
              </div>
            </div>
            <div className="space-y-2">
              <Label htmlFor="pay-notes">Notes</Label>
              <Textarea
                id="pay-notes"
                placeholder="Additional notes..."
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
                rows={2}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowRegister(false)}>Cancel</Button>
            <Button
              onClick={handleRegister}
              disabled={!athleteId || !amount || !paymentDate || registerPayment.isPending}
            >
              {registerPayment.isPending ? 'Registering...' : 'Register'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
