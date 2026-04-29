'use client';

import { useState } from 'react';
import { PageHeader } from '@/components/common';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
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
import { Plus, DollarSign, Clock, MoreVertical, Ban } from 'lucide-react';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { usePlans, useCreatePlan, useDeactivatePlan } from '@/hooks';
import { useCoach, useAuth } from '@/providers';
import { PlanListItem, PlanType } from '@/types';
import { toast } from 'sonner';

const PLAN_TYPE_LABELS: Record<PlanType, string> = {
  Monthly: 'Monthly',
  Quarterly: 'Quarterly',
  SemiAnnual: 'Semi-Annual',
  Annual: 'Annual',
  Custom: 'Custom',
};

function PlanCard({ plan, onDeactivate }: { plan: PlanListItem; onDeactivate: (id: string) => void }) {
  return (
    <Card className={`relative ${!plan.isActive ? 'opacity-60' : ''}`}>
      <CardHeader className="pb-3">
        <div className="flex items-start justify-between">
          <div>
            <CardTitle className="text-lg">{plan.name}</CardTitle>
            <Badge variant={plan.isActive ? 'default' : 'secondary'} className="mt-1">
              {plan.isActive ? 'Active' : 'Inactive'}
            </Badge>
          </div>
          {plan.isActive && (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" size="sm" className="h-8 w-8 p-0">
                  <MoreVertical className="h-4 w-4" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DropdownMenuItem
                  onClick={() => onDeactivate(plan.id)}
                  className="text-red-600"
                >
                  <Ban className="mr-2 h-4 w-4" />
                  Deactivate
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          )}
        </div>
      </CardHeader>
      <CardContent>
        <div className="space-y-3">
          <div className="flex items-center gap-2 text-2xl font-bold text-slate-900">
            <DollarSign className="h-5 w-5 text-green-600" />
            {plan.price.toFixed(2)}
            <span className="text-sm font-normal text-slate-500">{plan.currency}</span>
          </div>
          <div className="flex items-center gap-2 text-sm text-slate-600">
            <Clock className="h-4 w-4" />
            {PLAN_TYPE_LABELS[plan.planType]}
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

export default function PlansPage() {
  const { user } = useAuth();
  const { coach } = useCoach();
  const coachId = user?.coachId ?? coach?.id ?? '';
  const [showCreate, setShowCreate] = useState(false);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [price, setPrice] = useState('');
  const [currency, setCurrency] = useState('USD');
  const [durationMonths, setDurationMonths] = useState('1');

  const { data: plans, isLoading } = usePlans({ coachId });
  const createPlan = useCreatePlan();
  const deactivatePlan = useDeactivatePlan();

  const resetForm = () => {
    setName('');
    setDescription('');
    setPrice('');
    setCurrency('USD');
    setDurationMonths('1');
  };

  const handleCreate = () => {
    if (!coachId || !name || !price) return;

    createPlan.mutate(
      {
        coachId,
        name,
        description: description || undefined,
        price: parseFloat(price),
        currency,
        durationInMonths: parseInt(durationMonths),
      },
      {
        onSuccess: () => {
          toast.success('Plan created successfully');
          setShowCreate(false);
          resetForm();
        },
        onError: () => {
          toast.error('Failed to create plan');
        },
      }
    );
  };

  const handleDeactivate = (id: string) => {
    deactivatePlan.mutate(id, {
      onSuccess: () => toast.success('Plan deactivated'),
      onError: () => toast.error('Failed to deactivate plan'),
    });
  };

  const activePlans = plans?.filter((p) => p.isActive) || [];
  const inactivePlans = plans?.filter((p) => !p.isActive) || [];

  return (
    <div>
      <PageHeader
        title="Plans"
        description="Manage your coaching plans and pricing"
        actions={
          <Button onClick={() => setShowCreate(true)}>
            <Plus className="w-4 h-4 mr-2" />
            New Plan
          </Button>
        }
      />

      {isLoading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {[1, 2, 3].map((i) => (
            <Card key={i}>
              <CardHeader><Skeleton className="h-6 w-32" /></CardHeader>
              <CardContent><Skeleton className="h-10 w-24" /></CardContent>
            </Card>
          ))}
        </div>
      ) : plans && plans.length > 0 ? (
        <div className="space-y-8">
          {activePlans.length > 0 && (
            <div>
              <h2 className="text-lg font-semibold text-slate-900 mb-4">
                Active Plans ({activePlans.length})
              </h2>
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {activePlans.map((plan) => (
                  <PlanCard key={plan.id} plan={plan} onDeactivate={handleDeactivate} />
                ))}
              </div>
            </div>
          )}
          {inactivePlans.length > 0 && (
            <div>
              <h2 className="text-lg font-semibold text-slate-500 mb-4">
                Inactive Plans ({inactivePlans.length})
              </h2>
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {inactivePlans.map((plan) => (
                  <PlanCard key={plan.id} plan={plan} onDeactivate={handleDeactivate} />
                ))}
              </div>
            </div>
          )}
        </div>
      ) : (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <DollarSign className="h-12 w-12 text-slate-300 mb-4" />
            <h3 className="text-lg font-medium text-slate-900 mb-1">No plans yet</h3>
            <p className="text-slate-500 mb-4">Create your first coaching plan to get started</p>
            <Button onClick={() => setShowCreate(true)}>
              <Plus className="w-4 h-4 mr-2" />
              Create Plan
            </Button>
          </CardContent>
        </Card>
      )}

      {/* Create Plan Dialog */}
      <Dialog open={showCreate} onOpenChange={setShowCreate}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Create New Plan</DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label htmlFor="plan-name">Plan Name *</Label>
              <Input
                id="plan-name"
                placeholder="e.g. Monthly Coaching"
                value={name}
                onChange={(e) => setName(e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="plan-desc">Description</Label>
              <Textarea
                id="plan-desc"
                placeholder="What's included in this plan..."
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                rows={3}
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="plan-price">Price *</Label>
                <Input
                  id="plan-price"
                  type="number"
                  min="0"
                  step="0.01"
                  placeholder="99.00"
                  value={price}
                  onChange={(e) => setPrice(e.target.value)}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="plan-currency">Currency</Label>
                <Input
                  id="plan-currency"
                  value={currency}
                  onChange={(e) => setCurrency(e.target.value)}
                  placeholder="USD"
                />
              </div>
            </div>
            <div className="space-y-2">
              <Label htmlFor="plan-duration">Duration (months)</Label>
              <Input
                id="plan-duration"
                type="number"
                min="1"
                max="24"
                value={durationMonths}
                onChange={(e) => setDurationMonths(e.target.value)}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowCreate(false)}>
              Cancel
            </Button>
            <Button
              onClick={handleCreate}
              disabled={!name || !price || createPlan.isPending}
            >
              {createPlan.isPending ? 'Creating...' : 'Create Plan'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
