'use client';

import { useState } from 'react';
import { PageHeader } from '@/components/common';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
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
import { Plus, Search, XCircle } from 'lucide-react';
import { useAthleteSubscriptions, useCreateSubscription, useCancelSubscription, usePlans } from '@/hooks';
import { useAthletes } from '@/hooks';
import { useCoach, useAuth } from '@/providers';
import { SubscriptionStatus } from '@/types';
import { format } from 'date-fns';
import { toast } from 'sonner';

const STATUS_COLORS: Record<SubscriptionStatus, string> = {
  Active: 'bg-green-100 text-green-800',
  Paused: 'bg-yellow-100 text-yellow-800',
  Cancelled: 'bg-red-100 text-red-800',
  Expired: 'bg-slate-100 text-slate-800',
  PendingPayment: 'bg-orange-100 text-orange-800',
};

export default function SubscriptionsPage() {
  const { user } = useAuth();
  const { coach } = useCoach();
  const coachId = user?.coachId ?? coach?.id ?? '';
  const [selectedAthleteId, setSelectedAthleteId] = useState('');
  const [showCreate, setShowCreate] = useState(false);
  const [showCancel, setShowCancel] = useState<string | null>(null);
  const [cancelReason, setCancelReason] = useState('');

  // Create form state
  const [newAthleteId, setNewAthleteId] = useState('');
  const [newPlanId, setNewPlanId] = useState('');
  const [newStartDate, setNewStartDate] = useState('');

  const { data: athletes } = useAthletes({ coachId });
  const { data: plans } = usePlans({ coachId });
  const { data: subscriptions, isLoading } = useAthleteSubscriptions(selectedAthleteId);
  const createSubscription = useCreateSubscription();
  const cancelSubscription = useCancelSubscription();

  const handleCreate = () => {
    if (!newAthleteId || !newPlanId || !newStartDate) return;

    createSubscription.mutate(
      {
        athleteId: newAthleteId,
        planId: newPlanId,
        startDate: newStartDate,
      },
      {
        onSuccess: () => {
          toast.success('Subscription created');
          setShowCreate(false);
          setNewAthleteId('');
          setNewPlanId('');
          setNewStartDate('');
          if (newAthleteId === selectedAthleteId || !selectedAthleteId) {
            setSelectedAthleteId(newAthleteId);
          }
        },
        onError: () => toast.error('Failed to create subscription'),
      }
    );
  };

  const handleCancel = () => {
    if (!showCancel) return;
    cancelSubscription.mutate(
      { id: showCancel, reason: cancelReason || undefined },
      {
        onSuccess: () => {
          toast.success('Subscription cancelled');
          setShowCancel(null);
          setCancelReason('');
        },
        onError: () => toast.error('Failed to cancel subscription'),
      }
    );
  };

  return (
    <div>
      <PageHeader
        title="Subscriptions"
        description="Manage athlete subscriptions and billing periods"
        actions={
          <Button onClick={() => setShowCreate(true)}>
            <Plus className="w-4 h-4 mr-2" />
            New Subscription
          </Button>
        }
      />

      {/* Athlete Filter */}
      <div className="mb-6">
        <Label className="mb-2 block text-sm font-medium">Select Athlete</Label>
        <Select value={selectedAthleteId} onValueChange={setSelectedAthleteId}>
          <SelectTrigger className="max-w-sm">
            <SelectValue placeholder="Choose an athlete to view subscriptions..." />
          </SelectTrigger>
          <SelectContent>
            {athletes?.items?.map((a) => (
              <SelectItem key={a.id} value={a.id}>{a.fullName}</SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {!selectedAthleteId ? (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <Search className="h-12 w-12 text-slate-300 mb-4" />
            <h3 className="text-lg font-medium text-slate-900 mb-1">Select an athlete</h3>
            <p className="text-slate-500">Choose an athlete above to view their subscriptions</p>
          </CardContent>
        </Card>
      ) : isLoading ? (
        <Card>
          <CardContent className="space-y-3 py-6">
            {[1, 2].map((i) => (
              <Skeleton key={i} className="h-12 w-full" />
            ))}
          </CardContent>
        </Card>
      ) : subscriptions && subscriptions.length > 0 ? (
        <Card>
          <CardContent className="p-0">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Plan</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Start Date</TableHead>
                  <TableHead>End Date</TableHead>
                  <TableHead>Remaining</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {subscriptions.map((sub) => (
                  <TableRow key={sub.id}>
                    <TableCell className="font-medium">{sub.planName}</TableCell>
                    <TableCell>
                      <Badge className={STATUS_COLORS[sub.status]}>{sub.status}</Badge>
                    </TableCell>
                    <TableCell className="text-sm text-slate-600">
                      {format(new Date(sub.startDate), 'MMM d, yyyy')}
                    </TableCell>
                    <TableCell className="text-sm text-slate-600">
                      {format(new Date(sub.endDate), 'MMM d, yyyy')}
                    </TableCell>
                    <TableCell>
                      <span className={`text-sm font-medium ${sub.remainingDays <= 7 ? 'text-red-600' : 'text-slate-700'}`}>
                        {sub.remainingDays > 0 ? `${sub.remainingDays}d` : 'Expired'}
                      </span>
                    </TableCell>
                    <TableCell className="text-right">
                      {sub.status === 'Active' && (
                        <Button
                          variant="ghost"
                          size="sm"
                          className="text-red-600 hover:text-red-700"
                          onClick={() => setShowCancel(sub.id)}
                        >
                          <XCircle className="h-4 w-4 mr-1" />
                          Cancel
                        </Button>
                      )}
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
            <h3 className="text-lg font-medium text-slate-900 mb-1">No subscriptions</h3>
            <p className="text-slate-500 mb-4">This athlete has no subscriptions yet</p>
            <Button onClick={() => { setNewAthleteId(selectedAthleteId); setShowCreate(true); }}>
              <Plus className="w-4 h-4 mr-2" />
              Create Subscription
            </Button>
          </CardContent>
        </Card>
      )}

      {/* Create Subscription Dialog */}
      <Dialog open={showCreate} onOpenChange={setShowCreate}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Create Subscription</DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label>Athlete *</Label>
              <Select value={newAthleteId} onValueChange={setNewAthleteId}>
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
            <div className="space-y-2">
              <Label>Plan *</Label>
              <Select value={newPlanId} onValueChange={setNewPlanId}>
                <SelectTrigger>
                  <SelectValue placeholder="Select plan..." />
                </SelectTrigger>
                <SelectContent>
                  {plans?.filter((p) => p.isActive).map((p) => (
                    <SelectItem key={p.id} value={p.id}>
                      {p.name} — ${p.price} {p.currency}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label htmlFor="sub-start">Start Date *</Label>
              <Input
                id="sub-start"
                type="date"
                value={newStartDate}
                onChange={(e) => setNewStartDate(e.target.value)}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowCreate(false)}>Cancel</Button>
            <Button
              onClick={handleCreate}
              disabled={!newAthleteId || !newPlanId || !newStartDate || createSubscription.isPending}
            >
              {createSubscription.isPending ? 'Creating...' : 'Create'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Cancel Subscription Dialog */}
      <Dialog open={!!showCancel} onOpenChange={(open) => { if (!open) setShowCancel(null); }}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Cancel Subscription</DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <p className="text-slate-600">Are you sure you want to cancel this subscription? This action cannot be undone.</p>
            <div className="space-y-2">
              <Label htmlFor="cancel-reason">Reason (optional)</Label>
              <Input
                id="cancel-reason"
                placeholder="Why is this being cancelled?"
                value={cancelReason}
                onChange={(e) => setCancelReason(e.target.value)}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowCancel(null)}>Keep Active</Button>
            <Button
              variant="destructive"
              onClick={handleCancel}
              disabled={cancelSubscription.isPending}
            >
              {cancelSubscription.isPending ? 'Cancelling...' : 'Cancel Subscription'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
